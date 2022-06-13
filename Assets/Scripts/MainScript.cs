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
            InstrumentSettings
        }
        public UserState currentState;
        public Text InstructionText;

        private Camera arCamera;
        private List<FMOD.Studio.EventInstance> fmodInstances;
        private List<GameObject> GoosePrefabs;
        private List<string> names = new List<string>()
        {
            "Soprano",
            "Alto",
            "Tenor",
            "Bass"
        };
        private int counter = 0;
        private bool isDelay = false;
        private Goose selectedGoose;
        private bool holding;

        public void onValueChange()
        {
            float newScale = PitchSlider.value;
            Debug.Log($"newScale: {newScale}");
            //selectedGoose.pitchScale = PitchSlider.value + 0.5f;
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
            //if (Input.touchCount > 0 && currentState == UserState.InstrumentSettings)
            //{
            //    //InstructionText.text = Input.GetTouch(0).position.ToString();
            //    if (toggleInstrument != null && m_RaycastManager.Raycast(Input.GetTouch(0).position, s_Hits) && Input.GetTouch(0).position.y > 450 && Input.GetTouch(0).position.y < 2000)
            //    {
            //        var hitPose = s_Hits[0].pose;
            //        var rot = hitPose.rotation.eulerAngles + rotDeg[toggleID] * Vector3.up;
            //        toggleInstrument.gameObject.SetActive(true);
            //        toggleInstrument.transform.position = hitPose.position;
            //        toggleInstrument.transform.rotation = Quaternion.Euler(rot);
            //        toggleInstrument.activate();
            //        toggleInstrument = null;
            //        menuController.deselectToggle();
            //    }

            //    if (Input.GetTouch(0).phase == TouchPhase.Began)
            //    {
            //        Ray ray = arCamera.ScreenPointToRay(Input.GetTouch(0).position);
            //        RaycastHit hit;
            //        if (Physics.Raycast(ray, out hit))
            //        {
            //            var instrumentDetection = hit.transform.GetComponent<Instrument>();
            //            holding = instrumentDetection != null;
            //            if (holding)
            //            {
            //                selectedInstrument = instrumentDetection;
            //                toggleID = InstrumentPrefabs.FindIndex(obj => obj == selectedInstrument.gameObject);
            //            }
            //        }
            //    }
            //    if (Input.GetTouch(0).phase == TouchPhase.Ended)
            //    {
            //        if (holding)
            //        {
            //            instrumentSizeSlider.gameObject.SetActive(true);
            //            InstrumentDeactivateButton.SetActive(true);
            //            instrumentSizeSlider.value = (selectedInstrument.sizeScale - 0.5f) / 1.0f;
            //        }
            //        else
            //        {
            //            InstrumentDeactivateButton.SetActive(false);
            //            instrumentSizeSlider.gameObject.SetActive(false);
            //        }
            //        holding = false;
            //    }
            //    if (m_RaycastManager.Raycast(Input.GetTouch(0).position, s_Hits))
            //    {
            //        var hitPose = s_Hits[0].pose;
            //        var isTouchingSlider = false;
            //        m_ped.position = Input.GetTouch(0).position;
            //        List<RaycastResult> results = new List<RaycastResult>();
            //        m_gr.Raycast(m_ped, results);

            //        if (results.Count > 0)
            //        {
            //            if (results[0].gameObject.tag == "slider")
            //                isTouchingSlider = true;
            //        }
            //        if (holding && !isTouchingSlider)
            //        {
            //            var rot = hitPose.rotation.eulerAngles + rotDeg[toggleID] * Vector3.up;
            //            Move(hitPose.position, Quaternion.Euler(rot));
            //        }
            //    }
            //}
            if (Input.touchCount > 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    Ray ray = arCamera.ScreenPointToRay(Input.GetTouch(0).position);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        selectedGoose = hit.transform.GetComponent<Goose>();
                        holding = selectedGoose != null;
                    }
                }

                if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    holding = false;
                }

                if (m_RaycastManager.Raycast(Input.GetTouch(0).position, s_Hits))
                {
                    var hitPose = s_Hits[0].pose;
                    if (counter < 4)
                    {
                        //isDelay = true;
                        InstructionText.text = $"counter: {counter}";
                        Invoke("setNextObject", 1.0f);
                        InstructionText.text = "Hello";
                        //isDelay = false;
                        spawnedObject = Instantiate(prefabList[0], hitPose.position, hitPose.rotation);
                        spawnedObject.SetActive(true);
                        GoosePrefabs.Add(spawnedObject);
                        string choirName = names[counter];
                        InstructionText.text = $"Please Place the choir: {choirName}, updated counter: {counter}";
                        var fmodInstance = FMODUnity.RuntimeManager.CreateInstance($"event:/ARGoose/{choirName}");
                        spawnedObject.GetComponent<Goose>().Init(fmodInstance, choirName);
                        //fmodInstances.Add(fmodInstance);
                        fmodInstance.start();
                        fmodInstance.setPaused(true);
                        selectedGoose = spawnedObject.GetComponent<Goose>();
                        counter++;
                    }
                    else
                    {
                        if (holding)
                            Move(hitPose.position, hitPose.rotation);
                    }
                }
            }
        }

        private void setNextObject()
        {
            isDelay = false;
            InstructionText.text = $"setNextObject, counter: {counter}";
            if (counter < 4)
            {
                InstructionText.text = $"Place the {names[counter]}, updated counter: {counter}";
            }
        }

        void Move(Vector3 position, Quaternion rotation)
        {
            selectedGoose.transform.position = position;
            selectedGoose.transform.rotation = rotation;
            //instances[selectedIndex].set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(spawnedObject));
        }

        private void initGoose(Pose hitPose, int index)
        {
            spawnedObject = Instantiate(prefabList[index], hitPose.position, hitPose.rotation);
            spawnedObject.SetActive(false);
            GoosePrefabs.Add(spawnedObject);
            var fmodInstance = FMODUnity.RuntimeManager.CreateInstance("event:/ARGoose/" + names[index]);
            var goose = spawnedObject.GetComponent<Goose>();
            goose.Init(fmodInstance, names[index]);
            fmodInstances.Add(fmodInstance);
        }

        static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
        ARRaycastManager m_RaycastManager;
    }
}
