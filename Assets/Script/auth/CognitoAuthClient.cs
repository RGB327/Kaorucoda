using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// AWS Cognito Identity Provider의 공개 HTTPS JSON API를 UnityWebRequest로 직접 호출한다.
/// AWS SDK 불필요 — SigV4 서명 없이 호출 가능한 공개 액션(SignUp/InitiateAuth)만 사용.
/// </summary>
public class CognitoAuthClient
{
    private const string Target = "AWSCognitoIdentityProviderService";

    public IEnumerator SignUp(string region, string clientId, string username, string password, string email, Action<CognitoAuthResult> onDone)
    {
        var body = new SignUpRequest
        {
            ClientId = clientId,
            Username = username,
            Password = password,
            UserAttributes = new[] { new AttributeKV { Name = "email", Value = email } },
        };

        yield return Post(region, "SignUp", JsonUtility.ToJson(body),
            raw => onDone?.Invoke(new CognitoAuthResult { Success = true }),
            error => onDone?.Invoke(new CognitoAuthResult { Success = false, Error = error }));
    }

    public IEnumerator InitiateAuthPassword(string region, string clientId, string username, string password, Action<CognitoAuthResult> onDone)
    {
        var body = new InitiateAuthPasswordRequest
        {
            ClientId = clientId,
            AuthFlow = "USER_PASSWORD_AUTH",
            AuthParameters = new PasswordAuthParameters { USERNAME = username, PASSWORD = password },
        };

        yield return Post(region, "InitiateAuth", JsonUtility.ToJson(body),
            raw => onDone?.Invoke(ParseAuthResult(raw)),
            error => onDone?.Invoke(new CognitoAuthResult { Success = false, Error = error }));
    }

    public IEnumerator InitiateAuthRefresh(string region, string clientId, string refreshToken, Action<CognitoAuthResult> onDone)
    {
        var body = new InitiateAuthRefreshRequest
        {
            ClientId = clientId,
            AuthFlow = "REFRESH_TOKEN_AUTH",
            AuthParameters = new RefreshAuthParameters { REFRESH_TOKEN = refreshToken },
        };

        yield return Post(region, "InitiateAuth", JsonUtility.ToJson(body),
            raw => onDone?.Invoke(ParseAuthResult(raw)),
            error => onDone?.Invoke(new CognitoAuthResult { Success = false, Error = error }));
    }

    private static CognitoAuthResult ParseAuthResult(string raw)
    {
        var response = JsonUtility.FromJson<InitiateAuthResponse>(raw);
        var result = response?.AuthenticationResult;
        if (result == null || string.IsNullOrEmpty(result.AccessToken))
            return new CognitoAuthResult { Success = false, Error = "인증 응답에 토큰이 없습니다." };

        return new CognitoAuthResult
        {
            Success = true,
            AccessToken = result.AccessToken,
            IdToken = result.IdToken,
            RefreshToken = result.RefreshToken,
            ExpiresIn = result.ExpiresIn,
        };
    }

    private static IEnumerator Post(string region, string action, string jsonBody, Action<string> onSuccess, Action<string> onError)
    {
        string url = $"https://cognito-idp.{region}.amazonaws.com/";
        var request = new UnityWebRequest(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody)),
            downloadHandler = new DownloadHandlerBuffer(),
        };
        request.SetRequestHeader("Content-Type", "application/x-amz-json-1.1");
        request.SetRequestHeader("X-Amz-Target", $"{Target}.{action}");

        yield return request.SendWebRequest();

        string responseText = request.downloadHandler?.text;

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(ParseErrorMessage(responseText));
            yield break;
        }

        onSuccess?.Invoke(responseText);
    }

    private static string ParseErrorMessage(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "네트워크 오류";

        var err = JsonUtility.FromJson<CognitoErrorResponse>(raw);
        return !string.IsNullOrEmpty(err?.message) ? err.message : raw;
    }

    // ---- Cognito 요청/응답 원본 JSON 형태. JsonUtility는 필드명을 그대로 매칭하므로
    //      대소문자까지 Cognito 규격과 정확히 같아야 한다. ----

    [Serializable] private class AttributeKV { public string Name; public string Value; }

    [Serializable]
    private class SignUpRequest
    {
        public string ClientId;
        public string Username;
        public string Password;
        public AttributeKV[] UserAttributes;
    }

    [Serializable] private class PasswordAuthParameters { public string USERNAME; public string PASSWORD; }
    [Serializable] private class RefreshAuthParameters { public string REFRESH_TOKEN; }

    [Serializable]
    private class InitiateAuthPasswordRequest
    {
        public string ClientId;
        public string AuthFlow;
        public PasswordAuthParameters AuthParameters;
    }

    [Serializable]
    private class InitiateAuthRefreshRequest
    {
        public string ClientId;
        public string AuthFlow;
        public RefreshAuthParameters AuthParameters;
    }

    [Serializable]
    private class AuthenticationResult
    {
        public string AccessToken;
        public string IdToken;
        public string RefreshToken;
        public int ExpiresIn;
        public string TokenType;
    }

    [Serializable]
    private class InitiateAuthResponse
    {
        public AuthenticationResult AuthenticationResult;
    }

    [Serializable]
    private class CognitoErrorResponse
    {
        public string __type;
        public string message;
    }
}

/// <summary>CognitoAuthClient 호출 결과를 앱 내부에서 쓰기 편하게 정리한 형태.</summary>
public class CognitoAuthResult
{
    public bool Success;
    public string Error;
    public string AccessToken;
    public string IdToken;
    public string RefreshToken;
    public int ExpiresIn;
}
