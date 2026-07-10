using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 세이브 데이터 API(/save)를 호출하는 클라이언트. AuthManager의 AccessToken으로 인증하고,
/// 코루틴은 AuthManager(DontDestroyOnLoad MonoBehaviour) 위에서 돌린다.
/// 서버가 진짜 저장소라 로컬 세이브 파일을 조작해도 소용없다 — 이게 조작 방지의 핵심.
/// </summary>
public class SaveDataClient
{
    /// <summary>
    /// 세이브를 가져온다. saveBlob은 신규 유저처럼 세이브가 없으면 null.
    /// hadError는 "세이브가 없는 것"과 "네트워크/서버 오류로 못 가져온 것"을 구분하기 위한 값 —
    /// true면 saveBlob=null이어도 실제로는 세이브가 있을 수 있으니 새 게임으로 넘기면 안 된다.
    /// </summary>
    public void GetSave(Action<string, bool> onComplete)
    {
        AuthManager.Instance.EnsureFreshAccessToken(ready =>
        {
            if (!ready)
            {
                onComplete?.Invoke(null, true);
                return;
            }
            AuthManager.Instance.StartCoroutine(GetSaveRoutine(onComplete));
        });
    }

    public void PutSave(string saveBlobJson, Action<bool> onComplete)
    {
        AuthManager.Instance.EnsureFreshAccessToken(ready =>
        {
            if (!ready)
            {
                onComplete?.Invoke(false);
                return;
            }
            AuthManager.Instance.StartCoroutine(PutSaveRoutine(saveBlobJson, onComplete));
        });
    }

    /// <summary>서버에 저장된 세이브를 지운다. "삭제하기" 선택 시 사용.</summary>
    public void DeleteSave(Action<bool> onComplete)
    {
        AuthManager.Instance.EnsureFreshAccessToken(ready =>
        {
            if (!ready)
            {
                onComplete?.Invoke(false);
                return;
            }
            AuthManager.Instance.StartCoroutine(DeleteSaveRoutine(onComplete));
        });
    }

    private IEnumerator GetSaveRoutine(Action<string, bool> onComplete)
    {
        string url = $"{AuthManager.Instance.SaveApiBaseUrl}/save";
        using var request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", $"Bearer {AuthManager.Instance.AccessToken}");

        yield return request.SendWebRequest();

        if (request.responseCode == 404)
        {
            onComplete?.Invoke(null, false); // 아직 세이브가 없는 신규 계정 — 오류 아님
            yield break;
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[SaveDataClient] GetSave 실패: {request.responseCode} {request.error}");
            onComplete?.Invoke(null, true);
            yield break;
        }

        var response = JsonUtility.FromJson<GetSaveResponse>(request.downloadHandler.text);
        onComplete?.Invoke(response?.saveBlob, false);
    }

    private IEnumerator PutSaveRoutine(string saveBlobJson, Action<bool> onComplete)
    {
        string url = $"{AuthManager.Instance.SaveApiBaseUrl}/save";
        string body = JsonUtility.ToJson(new PutSaveRequest { saveBlob = saveBlobJson });

        using var request = new UnityWebRequest(url, "PUT")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
            downloadHandler = new DownloadHandlerBuffer(),
        };
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {AuthManager.Instance.AccessToken}");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            Debug.LogWarning($"[SaveDataClient] PutSave 실패: {request.responseCode} {request.error}");

        onComplete?.Invoke(request.result == UnityWebRequest.Result.Success);
    }

    private IEnumerator DeleteSaveRoutine(Action<bool> onComplete)
    {
        string url = $"{AuthManager.Instance.SaveApiBaseUrl}/save";
        using var request = UnityWebRequest.Delete(url);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", $"Bearer {AuthManager.Instance.AccessToken}");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            Debug.LogWarning($"[SaveDataClient] DeleteSave 실패: {request.responseCode} {request.error}");

        onComplete?.Invoke(request.result == UnityWebRequest.Result.Success);
    }

    [Serializable] private class GetSaveResponse { public string saveBlob; public int schemaVersion; public string updatedAt; }
    [Serializable] private class PutSaveRequest { public string saveBlob; }
}
