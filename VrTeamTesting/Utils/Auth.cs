using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using VrTeamTesting.API.Login.Model;

namespace VrTeamTesting.Utils
{
    public class AuthUtils
    {
        public static string GenerateSecurityToken(string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Common.Environment("JwtSecret"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, email)
                }),
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static string CreateToken(Session user)
        {
            if (user == null)
                throw new ArgumentException(nameof(user));

            var claims = new[]
            {
                new Claim("firstname",user.firstname),
                new Claim("lastname",user.lastname),
                new Claim("email", user.email),
                new Claim("contact", user.contact_id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("PDv7DrqznYL6nv7DrqzjnQYO9JxIsWdcjnQYL6nu0f"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken("Chps360", "User",
                claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static ClaimsIdentity ReadToken(string tokenString)
        {

            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("PDv7DrqznYL6nv7DrqzjnQYO9JxIsWdcjnQYL6nu0f"));
                var handler = new JwtSecurityTokenHandler();
                var validations = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = "Chps360",
                    ValidateAudience = true,
                    ValidAudience = "User",
                    //ValidateLifetime = true
                };

                var identity = handler.ValidateToken(tokenString, validations, out var tokenSecure).Identity as ClaimsIdentity;

                return identity;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

#nullable enable
        public static Session? ValidateJWTUser(string token)
        {
            if (token == null)
                throw new Exceptions.UnauthorizedException("Invalid Session");
            try
            {
                ClaimsIdentity identity = ReadToken(token);
                if (identity == null)
                {
                    return null;
                }
                else
                {

                    var response = new Session()
                    {
                        firstname = identity.Claims.FirstOrDefault(c => c.Type == "firstname")?.Value,
                        lastname = identity.Claims.FirstOrDefault(c => c.Type == "lastname")?.Value,
                        email = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                        contact_id = Guid.Parse(identity.Claims.FirstOrDefault(c => c.Type == "contact").Value),
                        //company_id = Guid.Parse(identity.Claims.FirstOrDefault(c => c.Type == "company").Value),
                        usertype = (identity.Claims.FirstOrDefault(c => c.Type == "usertype")?.Value),
                        companyname = (identity.Claims.FirstOrDefault(c => c.Type == "companyname")?.Value),
                        token = token
                    };



                    return response;
                }
            }
            catch (SecurityTokenExpiredException ex)
            {
                throw new Exceptions.UnauthorizedException("Session Expired", ex);
            }
            catch (Exception ex)
            {
                throw new Exceptions.UnauthorizedException("Session Expired");
            }
        }
    }
}
