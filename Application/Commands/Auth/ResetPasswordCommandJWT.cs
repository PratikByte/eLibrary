using eLibrary.Application.Interfaces.Services;
using eLibrary.Domain.Entities;
using eLibrary.Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace eLibrary.Application.Commands.Auth;

public class ResetPasswordCommandJWT: IRequest<string>
{
   public string Token { get; set; }=string.Empty;
   public string NewPassword { get; set; } = string.Empty;
}
public class ResetPasswordHandlerJWT : IRequestHandler<ResetPasswordCommandJWT, string>
{
   private readonly ITokenService _tokenService;
   private  readonly EBookDBContext _context;
   private readonly IPasswordHasher<User> _passwordHasher;

   public ResetPasswordHandlerJWT(ITokenService tokenService, EBookDBContext context, IPasswordHasher<User> passwordHasher)
   { 
       _tokenService = tokenService;
       _context = context;
       _passwordHasher = passwordHasher;

   }

   public async Task<string> Handle(ResetPasswordCommandJWT request, CancellationToken cancellationToken)
   {
       var userId=_tokenService.ValidatePasswordResetToken(request.Token);
       if (userId == null)
           return "Invalid or expired token.";
       // Find the user by userId
       var user = await _context.Users.FindAsync(int.Parse(userId));
       if (user == null)
           return "User not found.";
       // Update the user's password
       user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
       // Save changes to the database
       await _context.SaveChangesAsync(cancellationToken);
       return "Password reset successful!";

   }


}

