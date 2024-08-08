using UnityEngine;
using TMPro;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using Pv.Unity;
using System.Collections.Generic;
using UnityEngine.Android;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ARTextBoxes : MonoBehaviour
{
    private string leftText = ".";
    private string rightText = ".";
    private Vector3 position = new Vector3(0.0f, 0.1f, 0.2f);

    private Image imageUI;
    private GameObject canvasObject;
    private Canvas canvas;

    private GoogleAPIRequest apiRequest;
    private TextMeshPro leftTextMesh;

    private TextMeshPro rightTextMesh;

    [SerializeField] TMP_FontAsset fontAsset;

    private PorcupineManager porcupineManager;

    private ElevenLabsTTS elevenLabsTTS;

    private void Awake()
    {
        canvasObject = new GameObject("AR Canvas");
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        canvas.transform.SetParent(transform);
        canvas.transform.localPosition = position;
        canvas.transform.localScale = Vector3.one * 0.00025f;

        GameObject imageObject = new GameObject("AR Image");
        imageObject.transform.SetParent(canvas.transform, false);
        imageUI = imageObject.AddComponent<Image>();
        imageUI.gameObject.SetActive(false);

        StartCoroutine(LoadImage());
    }

    private IEnumerator LoadImage()
    {
        string imagePath = Path.Combine(Application.streamingAssetsPath, "slide.png");
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imagePath);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("No se pudo cargar la imagen AR: " + imagePath);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            Sprite loadedSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            imageUI.sprite = loadedSprite;
            imageUI.SetNativeSize();
            Debug.Log($"Imagen AR cargada. Tamaño: {loadedSprite.rect.size}");
        }
    }


    private void Start()
    {
        apiRequest = GetComponent<GoogleAPIRequest>();
        if (apiRequest == null)
        {
            Debug.LogError("GoogleAPIRequest component not found!");
            return;
        }
        leftTextMesh = CreateTextBox(leftText, new Vector3(0.3f, 0.05f, 0.5f));
        rightTextMesh = CreateTextBox(rightText, new Vector3(0.8f, 0.05f, 0.5f));

        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }

        try{
            porcupineManager = PorcupineManager.FromKeywordPaths(Config.PORCUPINE_API_KEY,
            new List<string> { Path.Combine(Application.streamingAssetsPath, "gemini-speak_en_android_v3_0_0.ppn"), Path.Combine(Application.streamingAssetsPath, "gemini-translate_en_android_v3_0_0.ppn"), Path.Combine(Application.streamingAssetsPath, "gemini-slides_en_android_v3_0_0.ppn") }, wakeWordCallback, processErrorCallback: ErrorCallback);
            porcupineManager.Start();
        }
        catch (Exception e){}

        try{
            elevenLabsTTS = GetComponent<ElevenLabsTTS>();
        }
        catch (Exception e){}
    }

    private void ErrorCallback(Exception e)
    {
        Debug.Log("wakeWordCallback " + e.Message);
    }

    private async void wakeWordCallback(int keywordIndex)
    {
        Debug.Log("wakeWordCallback " + keywordIndex);
        if (keywordIndex == 0)
        {
            string text = await apiRequest.SendRequestWithText(rightTextMesh.text);
            elevenLabsTTS.GenerateAndPlaySpeech(text);
        }
        else if (keywordIndex == 1)
        {
            TakeScreenshot();
        }
        else if (keywordIndex == 2)
        {
            leftTextMesh.enabled = false;
            rightTextMesh.enabled = false;
            imageUI.gameObject.SetActive(true);
        }
    }

    private TextMeshPro CreateTextBox(string text, Vector3 pos)
    {
        GameObject textObj = new GameObject("TextBox");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = pos;

        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.font = fontAsset;
        tmp.fontSize = 0.15f;

        RectTransform rectTransform = tmp.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(1, 0.5f);

        return tmp;
    }

    public void TakeScreenshot()
    {
        StartCoroutine(CaptureScreenshotAndSendToAPI());
    }

    private IEnumerator CaptureScreenshotAndSendToAPI()
    {
        yield return new WaitForEndOfFrame();

        string base64Image = CaptureScreenshot();
        if (!string.IsNullOrEmpty(base64Image))
        {
            Task<Phrase> task = apiRequest.SendRequestWithImageData(base64Image);
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Result != null)
            {
                Phrase phrase = task.Result;
                string result = "";
                foreach (var word in phrase.words)
                {
                    result += $"{word.kanji}: <color=red>{word.hiragana}</color>: {word.translation} \n";
                }
                leftTextMesh.text = result + "\n";
                rightTextMesh.text = $"<color=red>{phrase.original_phrase}</color>\n{phrase.translated_phrase}";
            }
        }
    }

    private string CaptureScreenshot()
    {
        int width = Screen.width;
        int height = Screen.height;
        Texture2D screenshotTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        Rect rect = new Rect(0, 0, width, height);
        screenshotTexture.ReadPixels(rect, 0, 0);
        screenshotTexture.Apply();

        byte[] byteArray = screenshotTexture.EncodeToPNG();
        string base64Image = Convert.ToBase64String(byteArray);

        Destroy(screenshotTexture);

        return base64Image;
    }
}
