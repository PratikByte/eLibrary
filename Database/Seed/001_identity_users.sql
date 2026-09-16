-- Development/test seed only. Run against PostgreSQL database: elibrary
-- Both users use password: Elibrary@123
-- This script is repeatable and does not run automatically in production.

BEGIN;

INSERT INTO "Users" (
    "Id",
    "Email",
    "Username",
    "PasswordHash",
    "Role",
    "CreatedAt",
    "UpdatedAt",
    "CreatedBy",
    "UpdatedBy",
    "IsDeleted",
    "DeletedAt",
    "DeletedBy",
    "ProfileImageMongoId"
)
SELECT
    '11111111-1111-1111-1111-111111111111'::uuid,
    'pratik@elibrary.local',
    'Pratik',
    'AQMAAACghgEAEAAAACNNMtn1ZI3Q5ehatX4c2adM4fo0Z+ytaOnlEt1VGxvy8+cQPxxXm+Hz1/WC2a4DMg==',
    'Admin',
    timezone('utc', now()),
    timezone('utc', now()),
    NULL,
    NULL,
    FALSE,
    NULL,
    NULL,
    NULL
WHERE NOT EXISTS (
    SELECT 1 FROM "Users"
    WHERE "Username" = 'Pratik' OR "Email" = 'pratik@elibrary.local'
);

INSERT INTO "Users" (
    "Id",
    "Email",
    "Username",
    "PasswordHash",
    "Role",
    "CreatedAt",
    "UpdatedAt",
    "CreatedBy",
    "UpdatedBy",
    "IsDeleted",
    "DeletedAt",
    "DeletedBy",
    "ProfileImageMongoId"
)
SELECT
    '22222222-2222-2222-2222-222222222222'::uuid,
    'hemant@elibrary.local',
    'Hemant',
    'AQMAAACghgEAEAAAAMe4ztp4NtfAfEVwewg5Q9Jqa6O7YSHMg/VSmNUykl81m8S5rdkU3O6y3n5Tkb1Vjg==',
    'User',
    timezone('utc', now()),
    timezone('utc', now()),
    NULL,
    NULL,
    FALSE,
    NULL,
    NULL,
    NULL
WHERE NOT EXISTS (
    SELECT 1 FROM "Users"
    WHERE "Username" = 'Hemant' OR "Email" = 'hemant@elibrary.local'
);

COMMIT;

-- Login checks:
-- Pratik / Elibrary@123 (Admin)
-- Hemant / Elibrary@123 (User)
