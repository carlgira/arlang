using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.Text;

public class ElevenLabsTTS : MonoBehaviour
{
    private const string API_URL = "https://api.elevenlabs.io/v1/text-to-speech/";
    private const string VOICE_ID = "21m00Tcm4TlvDq8ikWAM";

    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void GenerateAndPlaySpeech(string text)
    {
        StartCoroutine(GenerateSpeechCoroutine(text));
    }

    private IEnumerator GenerateSpeechCoroutine(string text)
    {
        string url = API_URL + VOICE_ID;

        // Create JSON payload
        string jsonPayload = JsonUtility.ToJson(new TextToSpeechRequest { text = text });

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();

            www.SetRequestHeader("xi-api-key", Config.ELEVENLABS_API_KEY);
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("accept", "audio/mpeg");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
                Debug.LogError("Response Code: " + www.responseCode);
                Debug.LogError("Response Body: " + www.downloadHandler.text);
            }
            else
            {
                byte[] audioData = www.downloadHandler.data;
                string filePath = Path.Combine(Application.persistentDataPath, "speech.mp3");
                File.WriteAllBytes(filePath, audioData);

                StartCoroutine(PlayAudioFile(filePath));
            }
        }
    }

    private IEnumerator PlayAudioFile(string filePath)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error loading audio: " + www.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
    }
}

[System.Serializable]
public class TextToSpeechRequest
{
    public string text;
}