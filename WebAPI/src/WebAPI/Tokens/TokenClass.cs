﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebAPI.Tokens
{
    public class TokenClass
    {///<summary>
     ///Token实体
     ///</summary>
        public class TokenEntity
        {
            ///<summary>
            ///token字符串
            ///</summary>
            public string access_token { get; set; }
            ///<summary>
            ///过期时差
            ///</summary>
            public int expires_in { get; set; }
        }
        ///<summary>
        ///token提供属性
        ///</summary>
        public class TokenProviderOptions
        {
            ///<summary>
            ///发行人
            ///</summary>
            public string Issuer { get; set; }
            ///<summary>
            ///订阅者
            ///</summary>
            public string Audience { get; set; }
            ///<summary>
            ///过期时间间隔(s)
            ///</summary>
            public TimeSpan Expiration { get; set; } = TimeSpan.FromSeconds(300);
            ///<summary>
            ///签名证书
            ///</summary>
            public SigningCredentials SigningCredentials { get; set; }
        }

        ///<summary>
        ///Token提供类
        ///</summary>
        public class TokenProvider
        {
            readonly TokenProviderOptions _options;
            public TokenProvider(TokenProviderOptions options)
            {
                _options = options;
            }
            ///<summary>
            ///生成令牌
            ///</summary>
            ///<paramname="context">http上下文</param>
            ///<paramname="username">用户名</param>
            ///<paramname="password">密码</param>
            ///<paramname="role">角色</param>
            ///<returns></returns>
            public async Task<TokenEntity> GenerateToken(HttpContext context, string username, string password, string role)
            {
                var identity = await GetIdentity(username);
                if (identity == null)
                {
                    return null;
                }
                var now = DateTime.UtcNow;
                //声明
                var claims = new Claim[]
                {
new Claim(JwtRegisteredClaimNames.Sub,username),
new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
new Claim(JwtRegisteredClaimNames.Iat,ToUnixEpochDate(now).ToString(),ClaimValueTypes.Integer64),
new Claim(ClaimTypes.Role,role),
new Claim(ClaimTypes.Name,username)
                };
                //Jwt安全令牌
                var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(_options.Expiration),
                signingCredentials: _options.SigningCredentials);
                //生成令牌字符串
                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
                var response = new TokenEntity
                {
                    access_token = encodedJwt,
                    expires_in = (int)_options.Expiration.TotalSeconds
                };
                return response;
            }

            private static long ToUnixEpochDate(DateTime date)
            {
                return (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
            }
            ///<summary>
            ///查看令牌是否存在
            ///</summary>
            ///<paramname="username">用户名</param>
            ///<returns></returns>
            private Task<ClaimsIdentity> GetIdentity(string username)
            {
                return Task.FromResult(
                new ClaimsIdentity(new System.Security.Principal.GenericIdentity(username, "token"),
                new Claim[]{
new Claim(ClaimTypes.Name,username)
                }));
            }
        }
    }
}
