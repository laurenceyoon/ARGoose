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
        //GameObject m_PlacedPrefab;
        public List<GameObject> prefabList;


        /// <summary>
        /// The prefab to instantiate on touch.
        /// </summary>
        //public GameObject placedPrefab
        //{
        //    get { return m_PlacedPrefab; }
        //    set { m_PlacedPrefab = value; }
        //}

        /// <summary>
        /// The object instantiated as a result of a successful raycast intersection with a plane.
        /// </summary>
        public GameObject spawnedObject { get; private set; }
        public Slider PitchSlider;
        public GameObject playButton, InstructionObject, menuObject;
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
        private bool holding;

        public void onValueChange()
        {
            float newScale = PitchSlider.value;
            Debug.Log($"newScale: {newScale}");
            if (selectedGoose != null)
            {
                selectedGoose.GetComponent<Goose>().pitchScale = PitchSlider.value;
            }
            //selectedGoose.GetComponent<Goose>().pitchScale = PitchSlider.value;
            //Vector3 newSize = new Vector3(newScale * selectedInstrument.originalSize.x, newScale * selectedInstrument.originalSize.y, newScale * selectedInstrument.originalSize.z);
            //selectedInstrument.sizeScale = newScale;
            //selectedInstrument.transform.localScale = newSize;
            //InstructionText.text = selectedInstrument.name +" : "+ holding.ToString()+", "+ newScale.ToString()+", "+ instrumentSizeSlider.value.ToString();
        }

        public void playMusic()
        {
            Debug.Log($"play music!");
            foreach (var instance in GoosePrefabs)
            {
                instance.GetComponent<Goose>().play();
            }
            playButton.SetActive(false);
        }
        //{
        //    Debug.Log($"play music!");
        //    var fmodInstance = FMODUnity.RuntimeManager.CreateInstance("event:/ARGoose/Soprano");
        //    fmodInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(this.gameObject));
        //    fmodInstance.start();
        //    //fmodInstance.setPaused(true);
        //    fmodInstance.setPaused(false);
        //}

        void Awake()
        {
            m_RaycastManager = GetComponent<ARRaycastManager>();
            fmodInstances = new List<FMOD.Studio.EventInstance>();
            GoosePrefabs = new List<GameObject>();
            currentState = UserState.Initializing;
            counter = 0;
        }

        bool TryGetTouchPosition(out Vector2 touchPosition)
        {
            if (Input.touchCount > 0)
            {
                touchPosition = Input.GetTouch(0).position;
                return true;
            }

            touchPosition = default;
            return false;
        }

        void Update()
        {
            if (Input.touchCount > 0)
            {
                //var hitPose = s_Hits[0].pose;
                //InstructionText.text += $"\nhitPose: {hitPose}";
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    InstructionText.text = $"GetTouch(0).phase == TouchPhase.Began";
                    InstructionText.text += $"\nGetTouch(0): {Input.GetTouch(0)}";
                    holding = true;
                    if (!isDelay && m_RaycastManager.Raycast(Input.GetTouch(0).position, s_Hits))
                    {
                        var hitPose = s_Hits[0].pose;
                        if (counter < prefabList.Count)
                        {
                            isDelay = true;
                            Invoke("setNextObject", 1.0f);
                            InstructionText.text += $"\nPlace the {names[counter]}, updated counter: {counter}";
                            instantiateGoose(hitPose);
                            counter++;
                            isDelay = false;
                        }
                        //else
                        //{
                        //    InstructionText.text += $"\n!!!!Move Move!!!1111 selectedGoose: {selectedGoose.name}";
                        //    selectedGoose.transform.position = hitPose.position;
                        //}
                    }
                    //Ray ray = arCamera.ScreenPointToRay(Input.GetTouch(0).position);
                    //InstructionText.text += $"\nray: {ray}";
                    //RaycastHit hit;
                    //if (Physics.Raycast(ray, out hit))
                    //{
                    //    InstructionText.text += $"\nrhit: {hit}";
                    //    InstructionText.text += $"\nPhysics.Raycast(ray, out hit): {Physics.Raycast(ray, out hit)}";
                    //    selectedGoose = hit.transform.GetComponent<Goose>();
                    //    InstructionText.text += $"\nselectedGoose: {selectedGoose}";
                    //    holding = selectedGoose != null;
                    //}
                }
                if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    InstructionText.text += $"\nGetTouch(0).phase == TouchPhase.Ended";
                    holding = false;
                }
                if (selectedGoose != null)
                {
                    InstructionText.text += $"\nselectedGoose: {selectedGoose.name}";
                }
            }
            //if (Input.touchCount > 0 && !isDelay && m_RaycastManager.Raycast(Input.GetTouch(0).position, s_Hits))
            //{
            //    var hitPose = s_Hits[0].pose;
            //    if (counter < prefabList.Count)
            //    {
            //        isDelay = true;
            //        Invoke("setNextObject", 1.0f);
            //        instantiateGoose(hitPose);
            //        counter++;
            //    }
            //    else
            //    {
            //        if (holding)
            //            Move(hitPose.position, hitPose.rotation);
            //    }
            //}
        }

        private void instantiateGoose(Pose hitPose)
        {
            spawnedObject = Instantiate(prefabList[counter], hitPose.position, hitPose.rotation);
            spawnedObject.SetActive(true);
            GoosePrefabs.Add(spawnedObject);
            var fmodInstance = FMODUnity.RuntimeManager.CreateInstance("event:/ARGoose/" + names[counter]);
            spawnedObject.GetComponent<Goose>().Init(fmodInstance, names[counter]);
            fmodInstances.Add(fmodInstance);
            selectedGoose = spawnedObject;
        }

        private void setNextObject()
        {
            isDelay = false;
            InstructionText.text += $"\nsetNextObject, counter: {counter}";
            if (counter < prefabList.Count)
            {
                InstructionText.text += $"\nPlace the {names[counter]}, updated counter: {counter}";
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
