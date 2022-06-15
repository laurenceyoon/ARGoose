using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

namespace UnityEngine.XR.ARFoundation.Samples
{
    /// <summary>
    /// Listens for touch events and performs an AR raycast from the screen touch point.
    /// AR raycasts will only hit detected trackables like feature points and planes.
    ///
    /// If a raycast hits a trackable, the <see cref="placedPrefab"/> is instantiated
    /// and moved to the hit position.
    /// </summary>
    [RequireComponent(typeof(ARRaycastManager))]
    public class MainScript : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Instantiates this prefab on a plane at the touch location.")]
        public List<GameObject> prefabList;
        public GameObject spawnedObject { get; private set; }
        public Slider PitchSlider, FlangerSlider;
        public GameObject PlayController, EffectController, InstructionObject, PlayButton, StopButton;
        public enum UserState
        {
            Initializing,
            Ready,
            Playing,
            GooseSettings
        }
        public UserState currentState;
        public Text InstructionText;

        private Camera arCamera;
        private List<FMOD.Studio.EventInstance> fmodInstances;
        private List<GameObject> GoosePrefabs;
        private List<string> names = new List<string>()
        {
            "Bass",
            "Tenor",
            "Alto",
            "Soprano",
        };
        private int counter = 0;
        private bool isDelay = false;
        private GameObject selectedGoose;

        public void onPitchScaleChange()
        {
            float newScale = PitchSlider.value;
            float sizeRatio = newScale + 0.5f;
            if (GoosePrefabs != null && GoosePrefabs.Count > 0)
            {
                foreach (GameObject goose in GoosePrefabs)
                {
                    Goose gooseComponent = goose.GetComponent<Goose>();
                    gooseComponent.pitchScale = newScale;
                    Transform gooseBody = goose.transform.GetChild(0);
                    Vector3 originScale = gooseComponent.originalScale;
                    Vector3 newSize = new Vector3(gooseBody.localScale.x, originScale.y * sizeRatio, gooseBody.localScale.z);
                    gooseBody.localScale = newSize;
                }
            }
        }

        public void onFlangerScaleChange()
        {
            float newScale = FlangerSlider.value;
            float sizeRatio = newScale + 0.5f;
            if (GoosePrefabs != null && GoosePrefabs.Count > 0)
            {
                foreach (GameObject goose in GoosePrefabs)
                {
                    Goose gooseComponent = goose.GetComponent<Goose>();
                    gooseComponent.flangerScale = newScale;
                    Transform gooseBody = goose.transform.GetChild(0);
                    Vector3 originScale = gooseComponent.originalScale;
                    Vector3 newSize = new Vector3(originScale.x * sizeRatio, gooseBody.localScale.y, gooseBody.localScale.z);
                    gooseBody.localScale = newSize;
                }
            }
        }

        public void playMusic()
        {
            foreach (var instance in GoosePrefabs)
            {
                instance.GetComponent<Goose>().play();
            }
            PlayButton.SetActive(false);
            StopButton.SetActive(true);
        }

        public void stopMusic()
        {
            foreach (var instance in GoosePrefabs)
            {
                instance.GetComponent<Goose>().stop();
            }
            StopButton.SetActive(false);
            PlayButton.SetActive(true);
        }

        void Awake()
        {
            InstructionText.text = $"Place the {names[counter]}";
            m_RaycastManager = GetComponent<ARRaycastManager>();
            fmodInstances = new List<FMOD.Studio.EventInstance>();
            GoosePrefabs = new List<GameObject>();
            currentState = UserState.Initializing;
            counter = 0;
        }

        void Update()
        {
            if (counter == prefabList.Count) // Goose placement has been completed
            {
                transform.GetComponent<ARPlaneManager>().SetTrackablesActive(false);
            }

            if (Input.touchCount > 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    if (!isDelay && m_RaycastManager.Raycast(Input.GetTouch(0).position, s_Hits))
                    {
                        var hitPose = s_Hits[0].pose;
                        if (counter < prefabList.Count)
                        {
                            isDelay = true;
                            Invoke("setNextObject", 1.0f);
                            instantiateGoose(hitPose);
                            counter++;
                            isDelay = false;
                        }
                        if (counter == prefabList.Count)  // Goose placement has just been completed!
                        {
                            PlayController.SetActive(true);
                            EffectController.SetActive(true);
                            InstructionText.text = "Enjoy! 🦢❤️";
                            transform.GetComponent<ARPlaneManager>().SetTrackablesActive(false);
                        }
                    }
                }
            }
        }

        private void instantiateGoose(Pose hitPose)
        {
            spawnedObject = Instantiate(prefabList[counter], hitPose.position, hitPose.rotation);
            spawnedObject.SetActive(true);
            GoosePrefabs.Add(spawnedObject);
            var fmodInstance = FMODUnity.RuntimeManager.CreateInstance("event:/ARGoose/" + names[counter]);
            Vector3 originScale = spawnedObject.transform.GetChild(0).localScale;
            spawnedObject.GetComponent<Goose>().Init(fmodInstance, names[counter], originScale);
            fmodInstances.Add(fmodInstance);
            selectedGoose = spawnedObject;
        }

        private void setNextObject()
        {
            if (counter < prefabList.Count)
            {
                InstructionText.text = $"Place the {names[counter]}";
            }
        }

        void Move(Vector3 position, Quaternion rotation)
        {
            selectedGoose.transform.position = position;
            selectedGoose.transform.rotation = rotation;
        }

        static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
        ARRaycastManager m_RaycastManager;
    }
}
