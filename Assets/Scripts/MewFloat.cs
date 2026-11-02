using System.Collections;
using UnityEngine;

public class MewFloat : MonoBehaviour
{
    [SerializeField] Transform visual;   // the MewModel child

    [Header("Wander area (local to the lab)")]
    [SerializeField] Vector3 areaMin = new Vector3(-0.30f, 0.18f, 0.10f);
    [SerializeField] Vector3 areaMax = new Vector3( 0.30f, 0.40f, 0.30f);
    [SerializeField] float smoothTime = 1.2f;
    [SerializeField] float maxSpeed = 0.12f;
    [SerializeField] Vector2 pauseRange = new Vector2(0.5f, 2f);

    [Header("Floating feel")]
    [SerializeField] float bobHeight = 0.012f;
    [SerializeField] float bobSpeed = 2f;
    [SerializeField] float tilt = 120f;      // degrees of lean per m/s
    [SerializeField] float maxTilt = 15f;
    [SerializeField] float turnSpeed = 5f;

    Transform cam;
    Vector3 target, velocity;
    Vector3 visualBasePos, visualBaseScale, rootBaseScale;
    Quaternion visualBaseRot;
    float waitTimer, hop, squash, bobPhase;
    Coroutine reactRoutine;

    void Awake()
    {
        if (visual == null && transform.childCount > 0) visual = transform.GetChild(0);
        visualBasePos = visual.localPosition;
        visualBaseRot = visual.localRotation;
        visualBaseScale = visual.localScale;
        rootBaseScale = transform.localScale;
    }

    void OnEnable()  => LessonHUD.MewSpoke += React;
    void OnDisable() => LessonHUD.MewSpoke -= React;

    void Start()
    {
        cam = Camera.main != null ? Camera.main.transform : null;
        target = transform.localPosition;
        bobPhase = Random.value * 10f;
        StartCoroutine(Intro());
    }

    void Update()
    {
        // 1. Drift toward the current target, then pause and pick another
        Vector3 pos = transform.localPosition;
        if (Vector3.Distance(pos, target) < 0.02f)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f) PickTarget();
        }
        transform.localPosition = Vector3.SmoothDamp(pos, target, ref velocity, smoothTime, maxSpeed);

        // 2. Always face the player (yaw only)
        if (cam != null)
        {
            Vector3 dir = cam.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, Quaternion.LookRotation(dir), turnSpeed * Time.deltaTime);
        }

        // 3. Lean into the movement direction
        Vector3 worldVel = transform.parent != null ? transform.parent.TransformVector(velocity) : velocity;
        Vector3 lv = transform.InverseTransformDirection(worldVel);
        float roll  = Mathf.Clamp(-lv.x * tilt, -maxTilt, maxTilt);
        float pitch = Mathf.Clamp( lv.z * tilt, -maxTilt, maxTilt);
        visual.localRotation = Quaternion.Slerp(
            visual.localRotation, Quaternion.Euler(pitch, 0f, roll) * visualBaseRot, 6f * Time.deltaTime);

        // 4. Bobbing, hop and squash
        float bob = Mathf.Sin((Time.time + bobPhase) * bobSpeed) * bobHeight;
        visual.localPosition = visualBasePos + Vector3.up * (bob + hop);
        visual.localScale = visualBaseScale * (1f + squash);
    }

    void PickTarget()
    {
        for (int i = 0; i < 8; i++)
        {
            target = new Vector3(
                Random.Range(areaMin.x, areaMax.x),
                Random.Range(areaMin.y, areaMax.y),
                Random.Range(areaMin.z, areaMax.z));
            if (Vector3.Distance(target, transform.localPosition) > 0.12f) break;
        }
        waitTimer = Random.Range(pauseRange.x, pauseRange.y);
    }

    // Called whenever Mew "speaks"
    public void React()
    {
        if (!isActiveAndEnabled) return;
        if (reactRoutine != null) StopCoroutine(reactRoutine);
        reactRoutine = StartCoroutine(Hop());
    }

    IEnumerator Hop()
    {
        for (float x = 0; x < 1; x += Time.deltaTime * 2.5f)
        {
            float s = Mathf.Sin(x * Mathf.PI);
            hop = 0.04f * s;
            squash = 0.08f * s;
            yield return null;
        }
        hop = 0f;
        squash = 0f;
    }

    IEnumerator Intro()
    {
        transform.localScale = Vector3.zero;
        for (float x = 0; x < 1; x += Time.deltaTime / 0.8f)
        {
            transform.localScale = rootBaseScale * EaseOutBack(x);
            yield return null;
        }
        transform.localScale = rootBaseScale;
        React();
    }

    static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3) + c1 * Mathf.Pow(x - 1f, 2);
    }
}