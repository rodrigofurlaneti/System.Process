using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.AuthorizationHandlers;
using System.Process.Domain.Enums;
using System.Process.Domain.ValueObjects;
using System.Phoenix.DataAccess.Redis;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace System.Process.UnitTests.Application.AuthorizationHandlers
{
    public class UserDataAuthorizationHandlerTests
    {
        #region Properties
        private Mock<IDictionary<ValidationProcess, Func<UserIdentity, string, bool>>> Validations { get; set; }
        private Mock<IHttpContextAccessor> HttpContextAccessor { get; set; }
        private Mock<ILogger<UserDataAuthorizationHandler>> Logger { get; set; }
        private Mock<IRedisService> RedisService { get; set; }
        private Mock<IOptions<ProcessConfig>> Config { get; set; }
        private UserDataAuthorizationHandler UserDataAuthorizationHandler { get; set; }
        private string AuthorizationHeader { get; set; }
        private BindingFlags BindingFlags { get; set; }
        private MethodInfo MethodInfo { get; set; }
        private object MethodInfoResult { get; set; }
        private IDictionary<string, object> DecodeJwt { get; set; }
        #endregion

        #region Constructor

        public UserDataAuthorizationHandlerTests()
        {
            Validations = new Mock<IDictionary<ValidationProcess, Func<UserIdentity, string, bool>>>();

            HttpContextAccessor = new Mock<IHttpContextAccessor>();

            Logger = new Mock<ILogger<UserDataAuthorizationHandler>>();

            RedisService = new Mock<IRedisService>();

            Config = new Mock<IOptions<ProcessConfig>>();

            Config
                .Setup(x => x.Value)
                .Returns(new ProcessConfig()
                {
                    ShouldDisableJwtValidation = true
                });

            UserDataAuthorizationHandler = new UserDataAuthorizationHandler(HttpContextAccessor.Object, Logger.Object, RedisService.Object, Config.Object);

            AuthorizationHeader = "U8366kjey9zwJ3kKcz5Nx5ngUhvU9hD57q0AiLNHzwQTMIg19QCUKgqA1iQlnnbmL5sgPkj30swS9jxufHM9sSqN+BqIYFbYLEtfCpmaU6sU8xB9QUeFSzGaHVcQz1QSzCCjCj38HLEDK1IrUzo9A100f5awMZfsvOwdnAD4CfvVe6iHjGVQjcE94V1g8NyW15KjvkrUnVf1OYMj+UDWY22CfUSXa8lQE/w3L0YEcl5cPKBBJaXCzhymzh3BHt3fsNmNw7aIeEoZbC9yuprw/tF2mN20Bk385CNhQ0tiWo7hjINYuYqjSuoMzup0G1CgDQnPFRBUyuEYh8FkjE6OmYZX9e4btM0H9QNBxR0BHcFcyzO1UCGYxd02FdrCKKXroS/EwPdSEXiZm3ALXHHBMGX0s+6LSAel8B3vhkkcouZheTviQ1a3SGAeyUAxJ538aWVOfDs4lNM1hBwRUu+KKag/m9z2reNA8EF2JXxp/KTQVbOcDAvNkYxvY3imkIxDe2F6WNGXKZmoAZRnitJyosk7dInYcOvktP64+Tns2w+7PnHJzH/oY8Cm46Nowici5XrJI8wMOkRt4Kcf9/B3fwSs9EexwYyIXHl+yIx7tyVvd8ulec2WTc0WViSQSeGK6DzeK4wgZgRkROgIdAIx2lOYUgIMY9RfzJvRyn2SzYEAB09edtdeDr3FbbEd5MCzm03AjKe/rgTvCBlU0LZiGelBgwaSXPVf8/syq5EwMvWHO9KlGSOtv37ptT3Sg7CIW0lTvGlnf7IBaLZeb2kz06PzKR1G44fdGoiyCIkd0gWf7FhbUdpKNgiQtoS9bgiA00nGRO+g7aUclqj4CEEeruAEQNKulzQa5Kkr3bqZGwhc8QVDTTNsY970b6qCVzrWE+fKD/+O/ZIAOUPnbGl/D0q+T6nWT0n5Y/3rpJWpO918mdp6DnJVHZLzhl6WgzhVypmliEU22kiu5W4c4trdyxdOKbdU1XG1lE0ehngdVFfw9q50pK9ovj5KCnzXv95u0yktkNcY3TWictk6sP7sLeO6J+tg2bUUpVXSi6PMpwmFMHnWQLXaVEK6YJVB5VRdGgkiiHPejjA828uTumqKmZpD8zd8DZAyjBcmvtWUkQ9y+gOWJWyaLQmmCqUlc16zuk0/KMd4idAyo4D0ZRHYGmI6WjtXVs8yTnjMrCIq/rmCOM07DeOaJX7TIwC0TMGW/tI6itdMR9ZR8WbCRCAlr3/dm17h+sbkKRHlGZw9pGL8kv7A/DzRH2/yHrP+nIwa5HswQjTtpf809EDMS0hWNg6YgLC07KLtg9o/GGpOmU0t5HDKeTuWTixUUFi9/08EK7rR8QtsGPTSml2BvJqXhxE0/ux267BxZIrJQARDVf6GkBkpQm+Xqygu0lHN1ESJKQ6u7w2F3VUKNpD0QHO7mHiB5j1evOX7Y7lxhiXBoY7k4fR0hg8wRubalnRwjXMHdcAhJEshUbZnLDZfwlH4m5e41h4zoVs40wlyJP+jNvm5GdPO0Q+4WpKKua4UBc6h7J75xEsuKyD6LGcI6AeqeFmkSOa6KYttLb0iI4dhFfO/WR05FaSf9oH3zBnQDd83s9GfV67Tko0aUvV3lDc2iQKs2C1nJ98dRLSBrclBTGc5SV8cckP/9B3yKLWy+gqYlwzQ31RPIxl6eUPpvj3u1Z2SGysal3J/ChcYJUYxi6dgyjeKa4dLqboN5SpLybT17TfBm8r/rrXAJCnJ33fOfwaibuVfTWh2e+9m40ZGD1Oo=";

        }

        #endregion

        #region Tests
        [Trait("Unit", "Success")]
        [Fact(DisplayName = "Should Send Decode Jwt Success")]
        public void ShouldSendDecodeJwtSuccess()
        {
            HttpContextAccessor = new Mock<IHttpContextAccessor>();

            Logger = new Mock<ILogger<UserDataAuthorizationHandler>>();

            RedisService = new Mock<IRedisService>();

            Config = new Mock<IOptions<ProcessConfig>>();

            Config
                .Setup(x => x.Value)
                .Returns(new ProcessConfig()
                {
                    ShouldDisableJwtValidation = true
                });

            UserDataAuthorizationHandler = new UserDataAuthorizationHandler(HttpContextAccessor.Object, Logger.Object, RedisService.Object, Config.Object);

            AuthorizationHeader = "U8366kjey9zwJ3kKcz5Nx5ngUhvU9hD57q0AiLNHzwQTMIg19QCUKgqA1iQlnnbmL5sgPkj30swS9jxufHM9sSqN+BqIYFbYLEtfCpmaU6sU8xB9QUeFSzGaHVcQz1QSzCCjCj38HLEDK1IrUzo9A100f5awMZfsvOwdnAD4CfvVe6iHjGVQjcE94V1g8NyW15KjvkrUnVf1OYMj+UDWY22CfUSXa8lQE/w3L0YEcl5cPKBBJaXCzhymzh3BHt3fsNmNw7aIeEoZbC9yuprw/tF2mN20Bk385CNhQ0tiWo7hjINYuYqjSuoMzup0G1CgDQnPFRBUyuEYh8FkjE6OmYZX9e4btM0H9QNBxR0BHcFcyzO1UCGYxd02FdrCKKXroS/EwPdSEXiZm3ALXHHBMGX0s+6LSAel8B3vhkkcouZheTviQ1a3SGAeyUAxJ538aWVOfDs4lNM1hBwRUu+KKag/m9z2reNA8EF2JXxp/KTQVbOcDAvNkYxvY3imkIxDe2F6WNGXKZmoAZRnitJyosk7dInYcOvktP64+Tns2w+7PnHJzH/oY8Cm46Nowici5XrJI8wMOkRt4Kcf9/B3fwSs9EexwYyIXHl+yIx7tyVvd8ulec2WTc0WViSQSeGK6DzeK4wgZgRkROgIdAIx2lOYUgIMY9RfzJvRyn2SzYEAB09edtdeDr3FbbEd5MCzm03AjKe/rgTvCBlU0LZiGelBgwaSXPVf8/syq5EwMvWHO9KlGSOtv37ptT3Sg7CIW0lTvGlnf7IBaLZeb2kz06PzKR1G44fdGoiyCIkd0gWf7FhbUdpKNgiQtoS9bgiA00nGRO+g7aUclqj4CEEeruAEQNKulzQa5Kkr3bqZGwhc8QVDTTNsY970b6qCVzrWE+fKD/+O/ZIAOUPnbGl/D0q+T6nWT0n5Y/3rpJWpO918mdp6DnJVHZLzhl6WgzhVypmliEU22kiu5W4c4trdyxdOKbdU1XG1lE0ehngdVFfw9q50pK9ovj5KCnzXv95u0yktkNcY3TWictk6sP7sLeO6J+tg2bUUpVXSi6PMpwmFMHnWQLXaVEK6YJVB5VRdGgkiiHPejjA828uTumqKmZpD8zd8DZAyjBcmvtWUkQ9y+gOWJWyaLQmmCqUlc16zuk0/KMd4idAyo4D0ZRHYGmI6WjtXVs8yTnjMrCIq/rmCOM07DeOaJX7TIwC0TMGW/tI6itdMR9ZR8WbCRCAlr3/dm17h+sbkKRHlGZw9pGL8kv7A/DzRH2/yHrP+nIwa5HswQjTtpf809EDMS0hWNg6YgLC07KLtg9o/GGpOmU0t5HDKeTuWTixUUFi9/08EK7rR8QtsGPTSml2BvJqXhxE0/ux267BxZIrJQARDVf6GkBkpQm+Xqygu0lHN1ESJKQ6u7w2F3VUKNpD0QHO7mHiB5j1evOX7Y7lxhiXBoY7k4fR0hg8wRubalnRwjXMHdcAhJEshUbZnLDZfwlH4m5e41h4zoVs40wlyJP+jNvm5GdPO0Q+4WpKKua4UBc6h7J75xEsuKyD6LGcI6AeqeFmkSOa6KYttLb0iI4dhFfO/WR05FaSf9oH3zBnQDd83s9GfV67Tko0aUvV3lDc2iQKs2C1nJ98dRLSBrclBTGc5SV8cckP/9B3yKLWy+gqYlwzQ31RPIxl6eUPpvj3u1Z2SGysal3J/ChcYJUYxi6dgyjeKa4dLqboN5SpLybT17TfBm8r/rrXAJCnJ33fOfwaibuVfTWh2e+9m40ZGD1Oo=";

            BindingFlags = BindingFlags.Static | BindingFlags.NonPublic;

            MethodInfo = typeof(UserDataAuthorizationHandler).GetMethod("DecodeJwt", BindingFlags);

            //Act
            MethodInfoResult = MethodInfo.Invoke(UserDataAuthorizationHandler, new object[] { AuthorizationHeader });

            //Assert
            Assert.Null(MethodInfoResult);
        }
        #endregion
    }
}
