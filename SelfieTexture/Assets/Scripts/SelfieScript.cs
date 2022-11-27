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
    public GameObject SquareSprite;

    private Quaternion baseRotation3d;
    private Quaternion baseRotation2d;

    void Start()
    {
        Btn.onClick.AddListener(StartTakePhoto);

        baseRotation3d = Plane1.transform.rotation;
        baseRotation2d = rawImage1.transform.rotation;

        // Flip objects scale to negative because webcamTexture is mirrored
        Flip();

        // Editor or standalone
#if UNITY_EDITOR || UNITY_STANDALONE
        webcamTexture = new WebCamTexture();
        Plane1.GetComponent<Renderer>().material.mainTexture = webcamTexture;
        rawImage1.texture = webcamTexture;
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
        File.WriteAllBytes(Application.persistentDataPath + "-Photo.png", bytes);

        // Load photo after saving
        LoadPhoto(Application.persistentDataPath + "-Photo.png");
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
            // Create sprite from texture
            Sprite blankSprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                500.0f
            );
            // Apply sprite
            SquareSprite.GetComponent<SpriteRenderer>().sprite = blankSprite;
        }
    }

    void Update()
    {
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
        SquareSprite.transform.rotation =
            baseRotation2d * Quaternion.AngleAxis(webcamTexture.videoRotationAngle, Vector3.back);
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
        SquareSprite.transform.localScale = new Vector3(
            SquareSprite.transform.localScale.x * -1,
            SquareSprite.transform.localScale.y,
            SquareSprite.transform.localScale.z
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
        SquareSprite.transform.localScale = new Vector3(
            SquareSprite.transform.localScale.x,
            SquareSprite.transform.localScale.y * -1,
            SquareSprite.transform.localScale.z
        );
#endif
    }
}
