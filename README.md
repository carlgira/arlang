# Arlang

Arlang is an AR app powered by Gemini AI that analyzes images, translates Japanese into English, and tutors users in the language.

Below are the instructions to compile, build, and run the Arlang app on an Android device.

### Creating API Keys

#### ElevenLabs API

1. **Sign Up / Log In**:
   - Visit [ElevenLabs](https://www.elevenlabs.io/) and sign up for a free account, or log in if you already have one.

2. **Navigate to the API Section**:
   - After logging in, go to your account dashboard.
   - Find the API section, located in the user settings (Profile + API key).

3. **Generate API Key**:
   - Click on the option to generate a new API key.
   - Copy the generated API key and store it securely.

#### Porcupine (Picovoice) API for Wake Words

1. **Sign Up / Log In**:
   - Visit [Picovoice Console](https://console.picovoice.ai/) and sign up for a free account, or log in if you already have one.

2. **Generate the API Key**:
   - The access key is prominently displayed on the page (at the time of writing). If not, create an access key.

#### Using Gemini API with Vertex AI

1. **Google Cloud Account Setup**:
   - If you don’t have a Google Cloud account, sign up at [Google Cloud](https://cloud.google.com/).
   - Log in to your Google Cloud Console.

2. **Create a Project and Enable Billing**:
   - Go to the [Google Cloud Console](https://console.cloud.google.com/).
   - Click on the project drop-down and select “New Project.”
   - Fill in the project name and other required details, then click “Create.”
   - Enable billing for your project by navigating to the [Billing section](https://console.cloud.google.com/billing) and linking a billing account to your project.

3. **Enable Vertex AI API**:
   - Go to the [API Library](https://console.cloud.google.com/apis/library).
   - Search for “Vertex AI” and click on it.
   - Click the “Enable” button to enable the Vertex AI API.

4. **Login to gcloud CLI**:
   - Install and initialize the [gcloud CLI](https://cloud.google.com/sdk/docs/install).
   - Log in to your account:
     ```bash
     gcloud auth login
     ```

5. **Generate Access Token**:
   - Obtain an access token using gcloud:
     ```bash
     gcloud auth print-access-token
     ```

6. **Gemini API URL**:
   - Build the URL for the REST call by going to the [multimodal console](https://console.cloud.google.com/vertex-ai/generative/multimodal/create/text) and checking the code on the right to see the URL for the curl command.
     ```plaintext
     https://${API_ENDPOINT}/v1/projects/${PROJECT_ID}/locations/${LOCATION_ID}/publishers/google/models/${MODEL_ID}:streamGenerateContent
     ```
   - For all my tests, I used MODEL_ID=`gemini-1.5-pro-001`.

### Building the Project with Unity

1. **Download the Project**:
   - Clone the Arlang project from GitHub: [Arlang Repository](https://github.com/carlgira/arlang).
     ```bash
     git clone https://github.com/carlgira/arlang.git
     ```

2. **Open the Project in Unity**:
   - Open Unity Hub.
   - Click on the “Add” button and select the cloned project folder.

3. **Switch Platforms to Android**:
   - In Unity, go to `File > Build Settings`.
   - Select `Android` and click on `Switch Platform`.

4. **Config.cs File**

Create or open the `Config.cs` file in your Unity project's `Assets` directory with the following content:

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config : MonoBehaviour
{   
    public static readonly string GEMINI_API_URL = "<YOUR_GEMINI_API_URL>";
    public static readonly string GEMINI_API_KEY = "<YOUR_GCLOUD_ACCESS_TOKEN>";
    public static readonly string ELEVENLABS_API_KEY = "<YOUR_ELEVENLABS_API_KEY>";
    public static readonly string PORCUPINE_API_KEY = "<YOUR_PORCUPINE_API_KEY>";
}
```

Replace `<YOUR_GEMINI_API_URL>`, `<YOUR_GCLOUD_ACCESS_TOKEN>`, `<YOUR_ELEVENLABS_API_KEY>`, and `<YOUR_PORCUPINE_API_KEY>` with your actual API keys and access token.

5. **Build the Project**:
   - Ensure your phone is connected and in debug mode.
   - Click on `Build and Run`.
   - Follow the prompts to complete the build process.

### Testing
You can watch the [video](https://www.youtube.com/watch?v=EK30zRYwlkg) on YouTube for a detailed guide with all the instructions.

There are three voice commands to activate functions:

- **gemini translate**: It will take a picture, send it to Gemini for analysis, extract Japanese text, and display the results in AR in two text areas—one with the whole phrase translation and another with word-by-word translation.
- **gemini speak**: Sends the phrase to Gemini for an explanation, then reads it aloud. Ensure your volume is turned up.
- **gemini slides**: Displays a simple slide of the app’s architecture. Be sure to hide it again before translating again.

## Acknowledgements

* **Author** - [Carlos Giraldo](https://www.linkedin.com/in/carlos-giraldo-a79b073b/)
* **Last Updated Date** - August 2024 (Gemini API Developer Competition)
