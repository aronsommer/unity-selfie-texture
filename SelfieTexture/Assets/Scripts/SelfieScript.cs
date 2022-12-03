using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SelfieScript : MonoBehaviour
{
    private WebCamTexture webcamTexture;

    public Button Btn;
    public GameObject Plane1;
    public GameObject Plane2;
    public RawImage rawImage1;
    public RawImage rawImage2;
    public GameObject SquareSprite1;
    public GameObject SquareSprite2;
    public Material SpriteWebCamTextureMaterial;
    public GameObject Sphere1;
    public GameObject Sphere2;

    private Quaternion baseRotation3d;
    private Quaternion baseRotation3dsphere;
    private Quaternion baseRotation2d;

    void Start()
    {
        Btn.onClick.AddListener(StartTakePhoto);

        baseRotation3d = Plane1.transform.rotation;
#if UNITY_EDITOR || UNITY_STANDALONE
        baseRotation3dsphere = Quaternion.Euler(0, -90, 0);
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
baseRotation3dsphere = Quaternion.Euler(-90, 0, 0);
#endif
        baseRotation2d = rawImage1.transform.rotation;

        // Flip objects scale to negative because webcamTexture is mirrored
        Flip();

        // Editor or standalone
#if UNITY_EDITOR || UNITY_STANDALONE
        webcamTexture = new WebCamTexture();
        Plane1.GetComponent<Renderer>().material.mainTexture = webcamTexture;
        rawImage1.texture = webcamTexture;
        SpriteWebCamTextureMaterial.SetTexture("_BlendTex", webcamTexture);
        SquareSprite1.GetComponent<SpriteRenderer>().material = SpriteWebCamTextureMaterial;
        Sphere1.GetComponent<Renderer>().material.mainTexture = webcamTexture;
        webcamTexture.Play();

#endif
        // Android
#if UNITY_ANDROID && !UNITY_EDITOR
        WebCamDevice[] devices = WebCamTexture.devices;
        foreach (var device in devices)
        {
            if (device.isFrontFacing)
            {
                webcamTexture = new WebCamTexture(device.name);                
                Plane1.GetComponent<Renderer>().material.mainTexture = webcamTexture;
                rawImage1.texture = webcamTexture;
                SpriteWebCamTextureMaterial.SetTexture("_BlendTex", webcamTexture);
                SquareSprite1.GetComponent<SpriteRenderer>().material = SpriteWebCamTextureMaterial;
                Sphere1.GetComponent<Renderer>().material.mainTexture = webcamTexture;
                webcamTexture.Play();
            }
        }
#endif
    }

    void StartTakePhoto()
    {
        StartCoroutine(TakePhoto());
    }

    IEnumerator TakePhoto()
    {
        yield return new WaitForEndOfFrame();

        Texture2D photo = new Texture2D(webcamTexture.width, webcamTexture.height);
        photo.SetPixels(webcamTexture.GetPixels());
        photo.Apply();

        byte[] bytes = photo.EncodeToPNG();
        File.WriteAllBytes(Application.persistentDataPath + "/Selfie.png", bytes);

        // Load photo after saving
        LoadPhoto(Application.persistentDataPath + "/Selfie.png");
    }

    private void LoadPhoto(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.Log("Error");
        }
        if (System.IO.File.Exists(path))
        {
            // Create texture from photo
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(1000, 1000, TextureFormat.RGB24, false);
            texture.filterMode = FilterMode.Trilinear;
            texture.LoadImage(bytes);
            // Apply texture
            Plane2.GetComponent<Renderer>().material.mainTexture = texture;
            rawImage2.texture = texture;
            Sphere2.GetComponent<Renderer>().material.mainTexture = texture;
            // Create sprite from texture
            Sprite blankSprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                500.0f
            );
            // Apply sprite
            SquareSprite2.GetComponent<SpriteRenderer>().sprite = blankSprite;
        }
    }

    void Update()
    {
        // Correct the aspect ratio of raw image
        float videoRatio = (float)webcamTexture.width / (float)webcamTexture.height;
        rawImage1.GetComponent<AspectRatioFitter>().aspectRatio = videoRatio;
        rawImage2.GetComponent<AspectRatioFitter>().aspectRatio = videoRatio;

        // Correct the rotation of objects because webcamTexture rotation was different on Android
        // transform.rotation = baseRotation * Quaternion.AngleAxis(webcamTexture.videoRotationAngle, Vector3.up);
        Plane1.transform.rotation =
            baseRotation3d * Quaternion.AngleAxis(webcamTexture.videoRotationAngle, Vector3.up);
        Plane2.transform.rotation =
            baseRotation3d * Quaternion.AngleAxis(webcamTexture.videoRotationAngle, Vector3.up);
        rawImage1.transform.rotation =
            baseRotation2d * Quaternion.AngleAxis(webcamTexture.videoRotationAngle, Vector3.back);
        rawImage2.transform.rotation =
            baseRotation2d * Quaternion.AngleAxis(webcamTexture.videoRotationAngle, Vector3.back);
        SquareSprite1.transform.rotation =
            baseRotation2d * Quaternion.AngleAxis(webcamTexture.videoRotationAngle, Vector3.back);
        SquareSprite2.transform.rotation =
            baseRotation2d * Quaternion.AngleAxis(webcamTexture.videoRotationAngle, Vector3.back);
        Sphere1.transform.rotation =
            baseRotation3dsphere
            * Quaternion.AngleAxis(webcamTexture.videoRotationAngle, Vector3.back);
        Sphere2.transform.rotation =
            baseRotation3dsphere
            * Quaternion.AngleAxis(webcamTexture.videoRotationAngle, Vector3.back);
    }

    void Flip()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        Plane1.transform.localScale = new Vector3(
            Plane1.transform.localScale.x * -1,
            Plane1.transform.localScale.y,
            Plane1.transform.localScale.z
        );
        Plane2.transform.localScale = new Vector3(
            Plane2.transform.localScale.x * -1,
            Plane2.transform.localScale.y,
            Plane2.transform.localScale.z
        );
        rawImage1.GetComponent<RectTransform>().localScale = new Vector3(
            rawImage1.transform.localScale.x * -1,
            rawImage1.transform.localScale.y,
            rawImage1.transform.localScale.z
        );
        rawImage2.GetComponent<RectTransform>().localScale = new Vector3(
            rawImage2.transform.localScale.x * -1,
            rawImage2.transform.localScale.y,
            rawImage2.transform.localScale.z
        );
        SquareSprite2.transform.localScale = new Vector3(
            SquareSprite1.transform.localScale.x * -1,
            SquareSprite1.transform.localScale.y,
            SquareSprite1.transform.localScale.z
        );
        Sphere1.transform.localScale = new Vector3(
            Sphere1.transform.localScale.x * -1,
            Sphere1.transform.localScale.y,
            Sphere1.transform.localScale.z
        );
        Sphere2.transform.localScale = new Vector3(
            Sphere2.transform.localScale.x * -1,
            Sphere2.transform.localScale.y,
            Sphere2.transform.localScale.z
        );
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
        Plane1.transform.localScale = new Vector3(
            Plane1.transform.localScale.x,
            Plane1.transform.localScale.y,
            Plane1.transform.localScale.z * -1
        );
        Plane2.transform.localScale = new Vector3(
            Plane2.transform.localScale.x,
            Plane2.transform.localScale.y,
            Plane2.transform.localScale.z * -1
        );
        rawImage1.GetComponent<RectTransform>().localScale = new Vector3(
            rawImage1.transform.localScale.x,
            rawImage1.transform.localScale.y * -1,
            rawImage1.transform.localScale.z
        );
        rawImage2.GetComponent<RectTransform>().localScale = new Vector3(
            rawImage2.transform.localScale.x,
            rawImage2.transform.localScale.y * -1,
            rawImage2.transform.localScale.z
        );
        SquareSprite2.transform.localScale = new Vector3(
            SquareSprite1.transform.localScale.x,
            SquareSprite1.transform.localScale.y * -1,
            SquareSprite1.transform.localScale.z
        );
        Sphere1.transform.localScale = new Vector3(
            Sphere1.transform.localScale.x,
            Sphere1.transform.localScale.y * -1,
            Sphere1.transform.localScale.z
        );
        Sphere2.transform.localScale = new Vector3(
            Sphere2.transform.localScale.x,
            Sphere2.transform.localScale.y * -1,
            Sphere2.transform.localScale.z
        );
#endif
    }
}
