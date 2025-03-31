using PassthroughCameraSamples;
using System.Collections;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Linq;

/// <summary>
/// Assigns the <see cref="WebCamTexture"/> of the Quest camera
/// to the <see cref="Renderer"/> component of the current game object.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class WebcamTextureAssigner : MonoBehaviour
{
    /// <summary>
    /// Start coroutine
    /// </summary>
    /// <returns></returns>
    public string fixed_ip;
    private int writePort = 9999;
	private IPEndPoint writeEndPoint;
	private UdpClient writer;
    private Texture2D texture;
    WebCamTextureManager webCamTextureManager = null;
    WebCamTexture webCamTexture = null;

    // Hand tracking variables
    private OVRSkeleton leftHandSkeleton;
    private OVRSkeleton rightHandSkeleton;
    private bool handsInitialized;
    [System.Serializable]
    public class HandDataContainer
    {
        public HandData leftHand;
        public HandData rightHand;
    }

    [System.Serializable]
    public class HandData
    {
        public string handType;
        public BoneData[] bones;
    }

    [System.Serializable]
    public class BoneData
    {
        public int id;
        public float px, py, pz;
        public float rx, ry, rz, rw;
    }
    IEnumerator Start()
    {
        //wait until the WebCamTextureManager is found and is ready to provide a texture
        do
        {
            yield return null;
 
            //if the WebCamTextureManager is not found yet, find it
            if (webCamTextureManager == null)
            {
                webCamTextureManager = FindFirstObjectByType<WebCamTextureManager>();
            }
            //else, if we have it, try to get the texture of the camera of the headset
            else
            {
                webCamTexture = webCamTextureManager.WebCamTexture;
                // Initialize texture ONLY AFTER webCamTexture is valid
                if (webCamTexture != null && texture == null)
                {
                    texture = new Texture2D(
                        webCamTexture.width, 
                        webCamTexture.height, 
                        TextureFormat.RGBA32, 
                        false
                    );
                }
            }
        } while (webCamTexture == null);
 
        // Hand tracking initialization
        yield return InitializeHandTracking();

        // UDP initialization
        InitializeUDP();
    }

    void InitializeUDP(){
        // Read server_ip from a text file, if there is no file use fixed ip
        string path = Application.persistentDataPath.ToString() + "/metaCub_IP.txt";
        UnityEngine.Debug.Log("Ip path: " + path);

        // Check if the file exists
        string server_ip;
        if (System.IO.File.Exists(path))
        {
            // If file exists, read IP from the file
            server_ip = System.IO.File.ReadAllText(path).Trim();
            UnityEngine.Debug.Log("Using IP from file: " + server_ip);
            writeEndPoint = new IPEndPoint(IPAddress.Parse(server_ip), writePort);
		    writer = new UdpClient();
        }
        else
        {
            UnityEngine.Debug.Log("Cant find IP");
            Application.Quit();
        }
    }

    IEnumerator InitializeHandTracking()
    {
        while (true)
        {
            var skeletons = FindObjectsOfType<OVRSkeleton>();
            
            foreach (var skeleton in skeletons)
            {
                if (skeleton.GetSkeletonType() == OVRSkeleton.SkeletonType.HandLeft)
                {
                    leftHandSkeleton = skeleton;
                }
                else if (skeleton.GetSkeletonType() == OVRSkeleton.SkeletonType.HandRight)
                {
                    rightHandSkeleton = skeleton;
                }
            }

            if (leftHandSkeleton != null && rightHandSkeleton != null &&
                leftHandSkeleton.IsInitialized && rightHandSkeleton.IsInitialized)
            {
                handsInitialized = true;
                yield break;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    void Update()
    {
        // Convert the texture to a Texture2D
        if (webCamTexture != null) {
            texture.SetPixels32(webCamTexture.GetPixels32());
            texture.Apply();
            // Encode the texture to a compressed format (JPEG or PNG)
            byte[] compressedData = texture.EncodeToJPG(10); // 50 is the quality, adjust as needed
            // Send the compressed data over UDP
            writer.Send(compressedData, compressedData.Length, writeEndPoint);
        }
        // Send hand data
        if (handsInitialized)
        {
            SendHandData();
        }
    }

    void SendHandData()
    {
        // Ensure non-null HandData even if skeleton is null
        HandData left = GetHandData(leftHandSkeleton, "Left") ?? new HandData { handType = "Left", bones = new BoneData[0] };
        HandData right = GetHandData(rightHandSkeleton, "Right") ?? new HandData { handType = "Right", bones = new BoneData[0] };

        var container = new HandDataContainer
        {
            leftHand = left,
            rightHand = right
        };

        string json = JsonUtility.ToJson(container);
        byte[] jsonData = Encoding.UTF8.GetBytes(json);
        SendData(jsonData, 1);
    }

    HandData GetHandData(OVRSkeleton skeleton, string handType)
    {
        if (skeleton == null || !skeleton.IsInitialized) return null;

        var boneData = skeleton.Bones.Select(bone => new BoneData
        {
            id = (int)bone.Id,
            px = bone.Transform.position.x,
            py = bone.Transform.position.y,
            pz = bone.Transform.position.z,
            rx = bone.Transform.rotation.x,
            ry = bone.Transform.rotation.y,
            rz = bone.Transform.rotation.z,
            rw = bone.Transform.rotation.w
        }).ToArray();

        return new HandData
        {
            handType = handType,
            bones = boneData
        };
    }

    void SendData(byte[] data, byte dataType)
    {
        byte[] packet = new byte[data.Length + 1];
        packet[0] = dataType;
        System.Buffer.BlockCopy(data, 0, packet, 1, data.Length);
        writer.Send(packet, packet.Length, writeEndPoint);
    }

    void OnDestroy()
    {
        if (writer != null)
        {
            writer.Close();
            writer = null;
        }
    }
}
 