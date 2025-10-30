﻿using FairHire.Application.Auth.Models.Request;
using FairHire.Application.Auth.Models.Responsess;
using FairHire.Application.Jwt;
using FairHire.Domain;
using Microsoft.AspNetCore.Identity;

namespace FairHire.Application.Auth.Commnad;

public sealed class SignInCommand(JwtService jwtService, UserManager<User> userManager)
{
    public async Task<SignInResponse> ExecuteAsync(SignInRequest request, CancellationToken ct)
    {
        var email = request.Email?.Trim();
        var user = await userManager.FindByEmailAsync(email!);
        if (user is null)
            throw new InvalidOperationException("Invalid email or password.");

        var ok = await userManager.CheckPasswordAsync(user, request.Password);
        if (!ok)
            throw new InvalidOperationException("Invalid email or password.");

        var roles = await userManager.GetRolesAsync(user);

        var token = jwtService.IssueToken(user.Id, user.Email!, roles);

        return new() {Token = token, Id = user.Id };
    }
}
