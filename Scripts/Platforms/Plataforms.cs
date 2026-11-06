using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class plataforms : MonoBehaviour
{
    public enum Direction
    {
        Left, Right, Up, Down, Forward, Backward
    }

    public enum WindDirection
    {
        Forward, Backward, Right, Left, Up, Down
    }

    #region Movement

    [Header("Movement Settings")]
    public bool CanMove = true;

    [SerializeField] private Direction direction = Direction.Right;
    [SerializeField, Min(0f)] private float distance = 20f;
    [SerializeField, Min(0f)] private float speed = 2f;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private bool movingToEnd = true;

    public Vector3 DeltaThisFrame { get; private set; }

    #endregion

    #region Spikes

    [Header("Spikes Settings")]
    public bool CanUseSpikes = false;

    [Tooltip("Objeto hijo que contiene los pinchos.")]
    [SerializeField] private Transform spikesRoot;

    [SerializeField, Min(0f)] private float spikeHeight = 1f;
    [SerializeField, Min(0f)] private float spikeSpeed = 4f;

    [Header("Intermittent Spikes")]
    [SerializeField] private bool intermittentSpikes = false;

    [SerializeField, Min(0.01f)] private float spikesOnTime = 2f;
    [SerializeField, Min(0.01f)] private float spikesOffTime = 2f;

    [SerializeField] private bool startSpikesHidden = true;

    private Vector3 spikesHiddenPosition;
    private Vector3 spikesVisiblePosition;

    private bool spikesAreVisible;
    private float spikesTimer;
    private bool spikesInitialized;

    private Collider[] spikesColliders;
    private bool[] spikesColliderInitialStates;

    private bool spikesAreDangerous;

    #endregion

    #region Wind

    [Header("Wind Settings")]
    public bool CanUseWind = false;

    [SerializeField]
    private WindDirection windDirection =
        WindDirection.Forward;

    [Tooltip("Fuerza aplicada por el viento.")]
    [SerializeField, Min(0f)] private float windForce = 20f;

    [Tooltip("Longitud de la zona de viento.")]
    [SerializeField, Min(0.1f)] private float windRange = 5f;

    [Tooltip("Ancho de la zona de viento.")]
    [SerializeField, Min(0.1f)] private float windWidth = 2f;

    [Tooltip("Altura de la zona de viento.")]
    [SerializeField, Min(0.1f)] private float windHeight = 2f;

    [Header("Intermittent Wind")]
    [SerializeField] private bool intermittentWind = false;

    [SerializeField, Min(0.01f)] private float windOnTime = 3f;
    [SerializeField, Min(0.01f)] private float windOffTime = 5f;

    [Header("Wind Effects")]
    [SerializeField] private ParticleSystem windParticles;

    private BoxCollider windTrigger;
    private bool windIsActive;
    private float windTimer;

    private readonly Dictionary<Movement, HashSet<Collider>>
        windPlayerColliders =
            new Dictionary<Movement, HashSet<Collider>>();

    #endregion

    #region Initialization

    private void Awake()
    {
        SetupSpikes();
        SetupWind();
    }

    private void Start()
    {
        startPosition = transform.position;

        endPosition = startPosition + GetDirection() * distance;

        windTimer = 0f;

        UpdateWindCollider();

        SetWindState(CanUseWind && !intermittentWind);

        if (windIsActive && intermittentWind)
            windTimer = windOnTime;
    }

    private void Update()
    {
        UpdateMovement();
        UpdateSpikes();
        UpdateWind();
    }

    #endregion

    #region Movement Logic

    private void UpdateMovement()
    {
        DeltaThisFrame = Vector3.zero;

        if (!CanMove || distance <= 0f || speed <= 0f)
            return;

        Vector3 previousPosition = transform.position;

        Vector3 target = movingToEnd ? endPosition : startPosition;

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        DeltaThisFrame =
            transform.position - previousPosition;

        if ((transform.position - target).sqrMagnitude < 0.000001f)
        {
            transform.position = target;

            DeltaThisFrame = transform.position - previousPosition;

            movingToEnd = !movingToEnd;
        }
    }

    private Vector3 GetDirection()
    {
        return direction switch
        {
            Direction.Left => Vector3.left,
            Direction.Right => Vector3.right,
            Direction.Up => Vector3.up,
            Direction.Down => Vector3.down,
            Direction.Forward => Vector3.forward,
            Direction.Backward => Vector3.back,
            _ => Vector3.right
        };
    }

    #endregion

    #region Spikes Logic

    private void SetupSpikes()
    {
        spikesInitialized = false;

        if (spikesRoot == null)
            return;

        if (spikesRoot == transform ||
            !spikesRoot.IsChildOf(transform))
        {
            Debug.LogError(
                "Spikes Root debe ser un objeto hijo de la plataforma.",
                this
            );

            spikesRoot = null;
            return;
        }

        spikesHiddenPosition = spikesRoot.localPosition;

        spikesVisiblePosition = spikesHiddenPosition + Vector3.up * spikeHeight;

        spikesAreVisible = CanUseSpikes && !startSpikesHidden;

        spikesTimer = spikesAreVisible ? spikesOnTime : spikesOffTime;

        spikesColliders = spikesRoot.GetComponentsInChildren<Collider>(true);

        spikesColliderInitialStates = new bool[spikesColliders.Length];

        for (int i = 0; i < spikesColliders.Length; i++)
        {
            spikesColliderInitialStates[i] = spikesColliders[i].enabled;

            spikesColliders[i].enabled = false;
        }

        spikesAreDangerous = false;
        spikesInitialized = true;

        if (!spikesAreVisible)
            spikesRoot.localPosition = spikesHiddenPosition;
    }

    private void UpdateSpikes()
    {
        if (!spikesInitialized || spikesRoot == null)
            return;

        if (!CanUseSpikes)
        {
            spikesAreVisible = false;
            spikesTimer = 0f;
        }
        else if (intermittentSpikes)
        {
            spikesTimer -= Time.deltaTime;

            if (spikesTimer <= 0f)
            {
                spikesAreVisible = !spikesAreVisible;

                spikesTimer = spikesAreVisible ? spikesOnTime : spikesOffTime;
            }
        }
        else
        {
            spikesAreVisible = true;
        }

        spikesVisiblePosition = spikesHiddenPosition + Vector3.up * spikeHeight;

        Vector3 target = spikesAreVisible ? spikesVisiblePosition : spikesHiddenPosition;

        if (!spikesAreVisible)
            SetSpikesDangerous(false);

        spikesRoot.localPosition = Vector3.MoveTowards(spikesRoot.localPosition, target, spikeSpeed * Time.deltaTime);

        bool fullyExtended = (spikesRoot.localPosition - spikesVisiblePosition).sqrMagnitude < 0.0001f;

        SetSpikesDangerous(CanUseSpikes && spikesAreVisible && fullyExtended);
    }

    private void SetSpikesDangerous(bool dangerous)
    {
        if (spikesColliders == null || spikesAreDangerous == dangerous)
        {
            return;
        }

        spikesAreDangerous = dangerous;

        for (int i = 0; i < spikesColliders.Length; i++)
        {
            if (spikesColliders[i] == null)
                continue;


            spikesColliders[i].enabled = dangerous && spikesColliderInitialStates[i];
        }
    }

    #endregion

    #region Wind Logic

    private Vector3 GetWindDirectionLocal()
    {
        return windDirection switch
        {
            WindDirection.Forward => Vector3.forward,
            WindDirection.Backward => Vector3.back,
            WindDirection.Right => Vector3.right,
            WindDirection.Left => Vector3.left,
            WindDirection.Up => Vector3.up,
            WindDirection.Down => Vector3.down,
            _ => Vector3.forward
        };
    }

    private Vector3 GetWindDirectionWorld()
    {
        return transform.TransformDirection(GetWindDirectionLocal()).normalized;
    }

    private void SetupWind()
    {
        Transform windArea = transform.Find("ZonaDeAire");

        if (windArea == null)
        {
            GameObject area = new GameObject("ZonaDeAire");

            windArea = area.transform;
            windArea.SetParent(transform, false);
        }

        windArea.localPosition = Vector3.zero;
        windArea.localRotation = Quaternion.identity;
        windArea.localScale = Vector3.one;

        windTrigger = windArea.GetComponent<BoxCollider>();

        if (windTrigger == null)
        {
            windTrigger = windArea.gameObject.AddComponent<BoxCollider>();
        }

        windTrigger.isTrigger = true;
        windTrigger.enabled = false;

        WindZoneDetect detector = windArea.GetComponent<WindZoneDetect>();

        if (detector == null)
        {
            detector = windArea.gameObject.AddComponent<WindZoneDetect>();
        }

        detector.Initialize(this);

        UpdateWindCollider();
    }

    private void UpdateWindCollider()
    {
        if (windTrigger == null)
            return;

        float width = Mathf.Max(0.1f, windWidth);
        float height = Mathf.Max(0.1f, windHeight);
        float range = Mathf.Max(0.1f, windRange);

        Vector3 dir = GetWindDirectionLocal();

        if (dir == Vector3.forward)
        {
            windTrigger.center =
                new Vector3(0f, 0f, range / 2f);

            windTrigger.size =
                new Vector3(width, height, range);
        }
        else if (dir == Vector3.back)
        {
            windTrigger.center =
                new Vector3(0f, 0f, -range / 2f);

            windTrigger.size =
                new Vector3(width, height, range);
        }
        else if (dir == Vector3.right)
        {
            windTrigger.center =
                new Vector3(range / 2f, 0f, 0f);

            windTrigger.size =
                new Vector3(range, height, width);
        }
        else if (dir == Vector3.left)
        {
            windTrigger.center =
                new Vector3(-range / 2f, 0f, 0f);

            windTrigger.size =
                new Vector3(range, height, width);
        }
        else if (dir == Vector3.up)
        {
            windTrigger.center =
                new Vector3(0f, range / 2f, 0f);

            windTrigger.size =
                new Vector3(width, range, height);
        }
        else
        {
            windTrigger.center =
                new Vector3(0f, -range / 2f, 0f);

            windTrigger.size =
                new Vector3(width, range, height);
        }
    }
    private void UpdateWind()
    {
        if (!CanUseWind)
        {
            SetWindState(false);
            windTimer = 0f;
            return;
        }

        if (!intermittentWind)
        {
            SetWindState(true);
            return;
        }

        windTimer -= Time.deltaTime;

        if (windTimer <= 0f)
        {
            bool nextState = !windIsActive;

            SetWindState(nextState);

            windTimer = nextState ? windOnTime : windOffTime;
        }
    }

    private void SetWindState(bool active)
    {
        bool shouldBeActive = CanUseWind && active;

        if (windIsActive == shouldBeActive)
            return;

        windIsActive = shouldBeActive;

        if (windTrigger != null)
            windTrigger.enabled = windIsActive;

        if (windParticles != null)
        {
            if (windIsActive)
            {
                if (!windParticles.isPlaying)
                    windParticles.Play();
            }
            else if (windParticles.isPlaying)
            {
                windParticles.Stop();
            }
        }

        if (!windIsActive)
        {
            foreach (Movement player in windPlayerColliders.Keys)
            {
                if (player != null)
                    player.StopWind(this);
            }

            windPlayerColliders.Clear();
        }
    }

    public void ProcessWind(Collider other)
    {
        if (!CanUseWind || !windIsActive || other == null)
            return;

        Movement player = other.GetComponentInParent<Movement>();

        if (player == null)
            return;

        if (!windPlayerColliders.TryGetValue(
            player,
            out HashSet<Collider> colliders))
        {
            colliders = new HashSet<Collider>();

            windPlayerColliders.Add(player, colliders);
        }

        colliders.Add(other);

        player.ApplyWind(this, GetWindDirectionWorld() * windForce);
    }

    public void ExitWind(Collider other)
    {
        if (other == null)
            return;

        Movement player = other.GetComponentInParent<Movement>();

        if (player == null)
            return;

        if (!windPlayerColliders.TryGetValue(
            player,
            out HashSet<Collider> colliders))
        {
            return;
        }

        colliders.Remove(other);

        if (colliders.Count == 0)
        {
            windPlayerColliders.Remove(player);
            player.StopWind(this);
        }
    }

    #endregion

    #region Validation

    private void OnValidate()
    {
        distance = Mathf.Max(0f, distance);
        speed = Mathf.Max(0f, speed);
        spikeHeight = Mathf.Max(0f, spikeHeight);
        spikeSpeed = Mathf.Max(0f, spikeSpeed);

        windWidth = Mathf.Max(0.1f, windWidth);
        windHeight = Mathf.Max(0.1f, windHeight);
        windRange = Mathf.Max(0.1f, windRange);

        spikesOnTime = Mathf.Max(0.01f, spikesOnTime);
        spikesOffTime = Mathf.Max(0.01f, spikesOffTime);

        windOnTime = Mathf.Max(0.01f, windOnTime);
        windOffTime = Mathf.Max(0.01f, windOffTime);

        if (!Application.isPlaying && windTrigger != null)
            UpdateWindCollider();
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        if (!CanUseWind)
            return;

        Gizmos.color = Color.cyan;

        Vector3 directionWorld = transform.TransformDirection(GetWindDirectionLocal()).normalized;

        Gizmos.DrawRay(transform.position, directionWorld * windRange);
    }

    #endregion
}