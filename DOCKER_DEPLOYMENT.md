# eLibrary Docker deployment

This setup builds the eLibrary application images on Windows, pushes them to a
private Docker Hub repository, and runs them on an Ubuntu laptop with Docker
Compose. PostgreSQL and MongoDB data are stored in named volumes. Only the
frontend and Gateway ports are published; the database and API service ports
stay on the private Compose network.

## Services and ports

| Service | Container port | Host port |
| --- | ---: | ---: |
| Frontend | 3001 | 3001 |
| Gateway | 8000 | 8000 |
| Identity API | 8001 | Private Compose network only |
| Book API | 8002 | Private Compose network only |
| PostgreSQL | 5432 | Private Compose network only |
| MongoDB | 27017 | Private Compose network only |

The Gateway forwards requests to `identity:8001` and `book:8002` using Docker's
internal DNS. The browser must use the Ubuntu laptop's LAN address, not those
container names.

## 1. Configure the deployment environment

Copy `.env.example` to `.env`. Set the Docker Hub username, image tag, strong
unique passwords, and a random JWT signing key. Do not commit `.env` or put
secrets in Dockerfiles or image build arguments.

Set `UBUNTU_LAN_IP` to the Ubuntu laptop's Wi-Fi IP. Compose binds the published
ports to that address rather than all network interfaces. Set
`PUBLIC_API_BASE_URL` to `http://<ubuntu-laptop-lan-ip>:8000` and
`FRONTEND_ORIGIN` to `http://<ubuntu-laptop-lan-ip>:3001`. The frontend API
address is compiled into its image, so changing the Ubuntu LAN IP requires
updating `.env` and rebuilding/pushing the frontend image. Use a LAN hostname
instead if your network provides one Windows can resolve.

For passwords used in connection strings, use long random alphanumeric values
to avoid connection-string delimiter characters. Keep the real `.env` file
private and transfer it to Ubuntu securely.

## 2. Create a private Docker Hub repository

Create a private repository named `elibrary` under the Docker Hub account in
`.env`. Create a Docker Hub access token and use it for `docker login` rather
than your account password. Grant push access on Windows and pull access on
Ubuntu where token permissions allow.

The image tags use this pattern:

```text
pratikdocker098/elibrary:identity-<IMAGE_TAG>
pratikdocker098/elibrary:book-<IMAGE_TAG>
pratikdocker098/elibrary:gateway-<IMAGE_TAG>
pratikdocker098/elibrary:frontend-<IMAGE_TAG>
```

These are four separate application images in the same Docker Hub repository.
Set `DOCKERHUB_USERNAME=pratikdocker098` and your chosen `IMAGE_TAG` in `.env`.
For example, with `IMAGE_TAG=uat`, the Identity image is
`pratikdocker098/elibrary:identity-uat`, and the Book image is
`pratikdocker098/elibrary:book-uat`. PostgreSQL and MongoDB use their official
images from Compose and are not built or pushed with the eLibrary application images.

## 3. Build and push on Windows

Use Docker Desktop in Linux-container mode. Check that its Linux image
architecture matches Ubuntu (`x86_64` corresponds to `linux/amd64`;
`aarch64` corresponds to `linux/arm64`). Then, from the repository root in
PowerShell:

```powershell
Copy-Item .env.example .env
# Edit .env with your Docker Hub username, image tag, LAN URLs, and secrets.
```

Use the exact image tag you plan to deploy. To test the whole stack locally
before committing to the Ubuntu LAN address, temporarily set
`PUBLIC_API_BASE_URL=http://localhost:8000`,
`FRONTEND_ORIGIN=http://localhost:3001`, and `UBUNTU_LAN_IP=127.0.0.1` in
`.env`. Build the four application images one at a time and save all build
output to a timestamped file under `logs/`:

```powershell
.\scripts\build-images.ps1
```

The script stops if a service image build fails. Review the log it prints, then
start the locally built images and check the database migration logs:

```powershell
docker compose --env-file .env -f compose.yaml -f compose.build.yaml up -d
docker compose ps
docker compose logs --tail=100 identity-migrate book-migrate
```

Open `http://localhost:3001` on Windows and test sign-in/API operations. The
build configuration packages EF Core migration bundles into the Identity and
Book images, so this test also validates fresh-database migrations. Stop the
test stack without deleting its volumes:

```powershell
docker compose -f compose.yaml -f compose.build.yaml down
```

By default, the script builds all four application images. To build just the
image for a changed service, pass its service name with `-Services`:

```powershell
.\scripts\build-images.ps1 -Services identity
.\scripts\build-images.ps1 -Services book
.\scripts\build-images.ps1 -Services gateway
.\scripts\build-images.ps1 -Services frontend
```

You can also build more than one selected service in a single run:

```powershell
.\scripts\build-images.ps1 -Services identity,book
```

The `-Services` values are Compose service names. They are not Docker Hub
image tags; the current `IMAGE_TAG` in `.env` is combined with each service
name to form the published tag.

Now set `UBUNTU_LAN_IP`, `PUBLIC_API_BASE_URL`, and `FRONTEND_ORIGIN` to the
Ubuntu laptop's LAN address. The frontend API address is compiled into its
image, so rebuild the frontend with the deployment settings. Rebuild all four
images if all services changed. Log into Docker Hub before pushing:

```powershell
# Build all four images:
.\scripts\build-images.ps1

# Or build only the changed service:
.\scripts\build-images.ps1 -Services frontend

docker login -u pratikdocker098
# Push all four images:
docker compose --env-file .env -f compose.yaml -f compose.build.yaml push identity book gateway frontend

# Or push only the changed service (example: frontend):
docker compose --env-file .env -f compose.yaml -f compose.build.yaml push frontend
```

## 4. Install Docker on Ubuntu

Install Docker Engine and the Docker Compose plugin using Docker's official
Ubuntu instructions for the Ubuntu release on the laptop. Verify the
installation:

```bash
docker --version
docker compose version
sudo docker run hello-world
```

You can run Docker with `sudo`; adding a user to the `docker` group is
optional and grants that user root-equivalent privileges.

## 5. Transfer Compose configuration and pull images

Copy only `compose.yaml` and the private `.env` file to a deployment directory
on Ubuntu. The build override and source code are not needed there. Then secure
the environment file and log into Docker Hub:

```bash
mkdir -p ~/elibrary
cd ~/elibrary
chmod 600 .env
docker login -u pratikdocker098
docker compose config --quiet
docker compose pull
```

The Compose file starts PostgreSQL and MongoDB first, waits for their health
checks, runs the Identity and Book migration bundles, then starts the APIs and
frontend. On subsequent image updates, the migration bundles apply any pending
migrations before their API starts.

## 6. Start and verify the application

```bash
docker compose up -d
docker compose ps
docker compose logs --tail=100
```

From Windows, while connected to the same home Wi-Fi, open:

```text
http://<ubuntu-laptop-lan-ip>:3001
```

If the page loads but API calls fail, check that the frontend image was built
with the Ubuntu LAN address, that `FRONTEND_ORIGIN` matches the browser's
frontend URL exactly, and that Windows can reach ports 3001 and 8000.

The published ports bind only to `UBUNTU_LAN_IP`, and the databases remain
unpublished. Do not forward application ports through the router. Docker
published ports can bypass UFW's usual filtering, so do not rely on UFW alone
to restrict Docker traffic. Devices on the same trusted Wi-Fi may be able to
reach the app; use router/client isolation or explicit Docker host firewall
rules if access must be limited to the Windows laptop alone.

## 7. Open a database and run a script

Run these commands from the folder containing `compose.yaml` and `.env`.
On Windows, use PowerShell in the repository root and include both Compose
files. On Ubuntu, use the deployment folder and the shorter commands shown
below. The PostgreSQL and MongoDB ports are private to Docker; connect with
`docker compose exec` rather than trying to connect to `localhost:5432` or
`localhost:27017` from Windows.

### PostgreSQL

Open an interactive `psql` prompt.

Windows PowerShell:

```powershell
docker compose --env-file .env -f compose.yaml -f compose.build.yaml exec postgres sh -lc 'psql -U "$POSTGRES_USER" -d eLibrary'
```

Ubuntu:

```bash
docker compose exec postgres sh -lc 'psql -U "$POSTGRES_USER" -d eLibrary'
```

At the `eLibrary=#` prompt, enter SQL commands. For example, `\dt` lists
tables and `\q` exits `psql`.

To execute a `.sql` file that exists on the host, pipe it into `psql`:

Windows PowerShell (from the repository root):

```powershell
Get-Content -Raw -Encoding utf8 .\Database\Seed\001_identity_users.sql |
  docker compose --env-file .env -f compose.yaml -f compose.build.yaml exec -T postgres sh -lc 'psql -v ON_ERROR_STOP=1 -U "$POSTGRES_USER" -d eLibrary'
```

Ubuntu (make sure the SQL file is present in the current directory):

```bash
docker compose exec -T postgres sh -lc 'psql -v ON_ERROR_STOP=1 -U "$POSTGRES_USER" -d eLibrary' < ./Database/Seed/001_identity_users.sql
```

`ON_ERROR_STOP=1` makes `psql` stop and return an error if a statement fails.
The supplied `Database/Seed/001_identity_users.sql` file is for development or
testing only; do not use it as a production setup script. The Identity and
Book schema migrations are run automatically by the `identity-migrate` and
`book-migrate` services when the stack starts.

### MongoDB

Open an interactive `mongosh` prompt. It asks for the MongoDB root password;
use the value of `MONGO_ROOT_PASSWORD` from your private `.env` file.

Windows PowerShell:

```powershell
docker compose --env-file .env -f compose.yaml -f compose.build.yaml exec mongo sh -lc 'mongosh --username "$MONGO_INITDB_ROOT_USERNAME" --authenticationDatabase admin'
```

Ubuntu:

```bash
docker compose exec mongo sh -lc 'mongosh --username "$MONGO_INITDB_ROOT_USERNAME" --authenticationDatabase admin'
```

After connecting, select the application database with `use elibrary_media`,
then use MongoDB commands such as `show collections`. Type `exit` to leave.

To execute a JavaScript file from the host, pipe it to `mongosh` and provide
the database name. Do not put the MongoDB password directly in the command:

Windows PowerShell:

```powershell
Get-Content -Raw -Encoding utf8 .\your-script.js |
  docker compose --env-file .env -f compose.yaml -f compose.build.yaml exec -T mongo sh -lc 'mongosh elibrary_media --quiet --username "$MONGO_INITDB_ROOT_USERNAME" --password "$MONGO_INITDB_ROOT_PASSWORD" --authenticationDatabase admin'
```

Ubuntu (make sure the JavaScript file is present in the current directory):

```bash
docker compose exec -T mongo sh -lc 'mongosh elibrary_media --quiet --username "$MONGO_INITDB_ROOT_USERNAME" --password "$MONGO_INITDB_ROOT_PASSWORD" --authenticationDatabase admin' < ./your-script.js
```

Before running any script that changes or deletes data, review it and make
sure you have a current database backup. Do not use `docker compose down -v`
to stop the application; that removes the database volumes.

## 8. Update the application

The Compose file uses one shared `IMAGE_TAG` for all four application images.
For example, with `DOCKERHUB_USERNAME=pratikdocker098` and `IMAGE_TAG=0.1.1`, the
image names are:

| Compose service | Docker Hub image |
| --- | --- |
| `identity` | `pratikdocker098/elibrary:identity-0.1.1` |
| `book` | `pratikdocker098/elibrary:book-0.1.1` |
| `gateway` | `pratikdocker098/elibrary:gateway-0.1.1` |
| `frontend` | `pratikdocker098/elibrary:frontend-0.1.1` |

Replace `0.1.1` with the `IMAGE_TAG` from `.env`. The same username and tag
must be used on Windows when building and pushing and on Ubuntu when pulling
and running.

### Update one service without changing `IMAGE_TAG`

Keep the current `IMAGE_TAG` in both `.env` files. On Windows, build and push
the changed service. Run `docker login -u pratikdocker098` first if you are not
already logged in, then enter your Docker Hub access token when prompted:

| Changed service | Windows build | Windows push |
| --- | --- | --- |
| Identity API | `.\scripts\build-images.ps1 -Services identity` | `docker compose --env-file .env -f compose.yaml -f compose.build.yaml push identity` |
| Book API | `.\scripts\build-images.ps1 -Services book` | `docker compose --env-file .env -f compose.yaml -f compose.build.yaml push book` |
| Gateway | `.\scripts\build-images.ps1 -Services gateway` | `docker compose --env-file .env -f compose.yaml -f compose.build.yaml push gateway` |
| Frontend | `.\scripts\build-images.ps1 -Services frontend` | `docker compose --env-file .env -f compose.yaml -f compose.build.yaml push frontend` |

Then, on Ubuntu, run the matching pull and recreate commands from the folder
containing `compose.yaml` and `.env`:

| Changed service | Ubuntu pull | Ubuntu recreate |
| --- | --- | --- |
| Identity API | `docker compose pull identity identity-migrate` | `docker compose up -d identity-migrate identity` |
| Book API | `docker compose pull book book-migrate` | `docker compose up -d book-migrate book` |
| Gateway | `docker compose pull gateway` | `docker compose up -d gateway` |
| Frontend | `docker compose pull frontend` | `docker compose up -d frontend` |

Identity and Book use migration jobs. Starting the matching migration and API
together makes Compose run the migration before starting that API. Check the
result with `docker compose ps` and, when needed,
`docker compose logs --tail=100 identity-migrate book-migrate`.

This single-service workflow updates an image at the existing tag. If you
change `IMAGE_TAG`, Compose expects the new tag for **all four** services, not
just the service you changed. Before changing the Ubuntu `.env` to a new tag,
build and push all four images with that tag on Windows:

```powershell
# Set the new IMAGE_TAG in .env first. Keep the Ubuntu deployment URLs there.
.\scripts\build-images.ps1
docker login -u pratikdocker098
docker compose --env-file .env -f compose.yaml -f compose.build.yaml push identity book gateway frontend
```

Then update the Ubuntu `.env` to that same `IMAGE_TAG` and run:

```bash
docker compose config --quiet
docker compose pull
docker compose up -d
docker compose ps
```

Do not change the Ubuntu tag until all four images with that tag have been
pushed, or Compose will be unable to pull the missing service images.

### If the Ubuntu laptop's LAN IP changes

Set `UBUNTU_LAN_IP` in the Ubuntu `.env` to the laptop's current Wi-Fi/LAN IP.
This is the host address Docker binds ports `3001` and `8000` to; it is not a
Docker Hub setting or an image tag. Set the Windows build `.env` to the same
IP, and update:

- `PUBLIC_API_BASE_URL=http://<new-ip>:8000` so the browser uses the new
  Gateway address. This value is compiled into the frontend, so rebuild and
  push the `frontend` image using the current `IMAGE_TAG`.
- `FRONTEND_ORIGIN=http://<new-ip>:3001` so the Gateway allows the frontend's
  new browser origin. This is runtime configuration, so the Gateway image
  does not need rebuilding.

On Ubuntu, after the frontend image has been pushed and the `.env` values
updated, apply the new host binding and Gateway CORS setting with:

```bash
docker compose pull frontend
docker compose up -d gateway frontend
docker compose ps
```

If you also change `IMAGE_TAG`, follow the all-four-images release procedure
above. From Windows, verify the site at `http://<new-ip>:3001`.

`docker compose down` does not remove the database volumes. Avoid
`docker compose down -v` and `docker system prune --volumes` unless you
intentionally want to delete database data. Back up PostgreSQL and MongoDB
volumes regularly.

This is an HTTP-only home-network deployment. Do not expose it to the public
internet; use HTTPS and a properly secured reverse proxy if that requirement
changes.

## Quick commands

### Build all images on Windows

```powershell
.\scripts\build-images.ps1
docker login -u pratikdocker098
docker compose --env-file .env -f compose.yaml -f compose.build.yaml push identity book gateway frontend
```

### Identity

```powershell
.\scripts\build-images.ps1 -Services identity
docker compose --env-file .env -f compose.yaml -f compose.build.yaml push identity
```

```bash
docker compose pull identity identity-migrate
docker compose up -d identity-migrate identity
```

### Book

```powershell
.\scripts\build-images.ps1 -Services book
docker compose --env-file .env -f compose.yaml -f compose.build.yaml push book
```

```bash
docker compose pull book book-migrate
docker compose up -d book-migrate book
```

### Gateway

```powershell
.\scripts\build-images.ps1 -Services gateway
docker compose --env-file .env -f compose.yaml -f compose.build.yaml push gateway
```

```bash
docker compose pull gateway
docker compose up -d gateway
```

### Frontend

```powershell
.\scripts\build-images.ps1 -Services frontend
docker compose --env-file .env -f compose.yaml -f compose.build.yaml push frontend
```

```bash
docker compose pull frontend
docker compose up -d frontend
```

### Frontend build with API IP reference

```powershell
$env:PUBLIC_API_BASE_URL = "http://1.10.32.32:8000"; .\scripts\build-images.ps1 -Services frontend
```
