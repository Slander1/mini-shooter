using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    
    [SerializeField] private Image pointer;
    [SerializeField] private ParticleSystem onGunParticleSystem;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject OnenemyparticleSystem;
    [SerializeField] private float moveSpeed = 1;
    [SerializeField] private float reload = 1f;
    [SerializeField] private float maxDistance = 5;
    

    private PlayerInput _playerInput;
    private CharacterController _controller;
    private Camera _cam;
    private float _lastShoot;
    private static readonly int SpeedMultiplyFloatName = Animator.StringToHash("SpeedMultiply");
    private static readonly int ShootTriggerName = Animator.StringToHash("Shoot");

    private static int Enemylayer => LayerMask.NameToLayer("Enemy");

    private void Awake()
    {
        _playerInput = new PlayerInput();
        _controller = GetComponent<CharacterController>();
        _cam = Camera.main;
    }
    
    private void OnEnable()
    {
        _playerInput.Enable();
    }

    private void OnDisable()
    {
        _playerInput.Disable();
    }

    private void Shoot(RaycastHit hit)
    {
        if (Time.time < _lastShoot + reload)
            return;
        
        _lastShoot = Time.time;
        //OnenemyparticleSystem
        //OnenemyparticleSystem.gameObject.
        GameObject explossion = Instantiate(OnenemyparticleSystem, hit.point, Quaternion.identity);
        
       // OnenemyparticleSystem.transform.forward = -hit.transform.forw;
        animator.SetTrigger(ShootTriggerName);
        Destroy(explossion, reload);
        onGunParticleSystem.Play();
        audioSource.Play();
    }

    private void Update()
    {
        MoveCharacter();
        MovePointer();
    }

    private void MovePointer()
    {
        var ray = new Ray(transform.position, transform.forward);
        var targetPosition = ray.GetPoint(5);
        var isRayCastCollision = Physics.Raycast(ray, out var hit, maxDistance);

        pointer.transform.position = _cam.WorldToScreenPoint(isRayCastCollision ? hit.point : targetPosition);
        pointer.color = Color.green;

        if (isRayCastCollision)
        {
            var isTargetEnemy = hit.collider.gameObject.layer == Enemylayer;

            pointer.color = isTargetEnemy ? Color.red : Color.blue;
            if (isTargetEnemy)
                Shoot(hit);
        }
    }

    private void MoveCharacter()
    {
        var movementInput = _playerInput.Player.Move.ReadValue<Vector2>();
        var aimInput = _playerInput.Player.Aim.ReadValue<Vector2>();
        var move = new Vector3(movementInput.x, 0f, movementInput.y);
        var aimMove = new Vector3(aimInput.x, 0f, aimInput.y);
        var angle = 0f;

        if(move != Vector3.zero)
            angle = Mathf.Lerp(-1, 1, (Mathf.Atan2(move.z, move.x) - Mathf.Atan2(aimMove.z, move.x)) / 180);
        animator.SetFloat(SpeedMultiplyFloatName, angle);
        _controller.Move(move * (Time.deltaTime * moveSpeed));
        if (aimMove != Vector3.zero)
            transform.forward = aimMove;
    }
}
