using UnityEngine;
using System.Collections;

public class Cat : MonoBehaviour
{

    public float speed = 4.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;

    private CharacterController CharController;

    private Vector3 destination;
    private enum CatState
    {
        Idle,
        Walking,
        Following
    };
    private CatState state = CatState.Following;
    private float stateTime = 0;
    private AudioClip[] sounds;
    private float soundCountdown;

	// Use this for initialization
	void Start ()
    {
        soundCountdown = Random.Range(3, 8);
        CharController = GetComponent<CharacterController>();

        var audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;
        }
         
        sounds = Resources.LoadAll<AudioClip>("Sounds");
	}

    GameObject FindPlayer()
    {
        return GameObject.FindGameObjectWithTag("Player");
    }

    Vector3 FindRandomNearbyLocation()
    {
        var attempts = 5;
        do
        {
            var angle = Random.Range(0, 360);
            var radius = Random.Range(2, 6);
            var position = new Vector3(Mathf.Cos(angle) * radius, 1, Mathf.Sin(angle));

            if (Physics.Raycast(new Ray(position, Vector3.down)))
            {
                return position;
            }
            attempts--;
            if (attempts < 0)
            {
                return transform.position;
            }
        } while (true);
    }

    Vector3 GetDestination()
    {
        return state == CatState.Walking ? destination : FindPlayer().transform.position;
    }

    void ChangeState(CatState newState)
    {
        state = newState;
        stateTime = Random.Range(1, 3);
    }

    AudioClip GetRandomClip()
    {
        return sounds[Random.Range(0, sounds.Length)];
    }
	
	// Update is called once per frame
	void Update ()
    {
        stateTime -= Time.deltaTime;
        soundCountdown -= Time.deltaTime;

        if (soundCountdown < 0)
        {
            var source = GetComponent<AudioSource>(); 
            source.PlayOneShot(GetRandomClip());

            soundCountdown = Random.Range(3, 8);
        }

        var lengthToPlayer = Vector3.Distance(FindPlayer().transform.position, transform.position);
        if (lengthToPlayer < 2)
        {
            ChangeState(CatState.Following);
        }

        moveDirection = Vector3.zero;
        if (state != CatState.Idle)
        {
            if (CharController.isGrounded)
            {
                var stateDestination = GetDestination();

                var toDestination = stateDestination - transform.position;
                toDestination = new Vector3(toDestination.x, 0, toDestination.z);
                var length = toDestination.magnitude;
                toDestination /= length;

                var cross = Vector3.Cross(toDestination, transform.forward);

                if (cross.y < -0.1)
                {
                    transform.Rotate(0, 180.0f * Time.deltaTime, 0);
                }
                else if (cross.y > 0.1)
                {
                    transform.Rotate(0, -180.0f * Time.deltaTime, 0);
                }

                if (length > 10 && state == CatState.Following)
                {
                    ChangeState(CatState.Idle);
                }

                if (length > 3 && Mathf.Abs(cross.y) < 0.3)
                {
                    moveDirection = transform.forward;
                    moveDirection *= speed;
                }
            }
        }

        if (state != CatState.Following)
        {
            if (stateTime < 0)
            {
                var newState = Random.value < 0.5f ? CatState.Idle : CatState.Walking; 
                ChangeState(newState);
                if (newState == CatState.Walking)
                {
                    destination = FindRandomNearbyLocation();
                }
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;
        CharController.Move(moveDirection * Time.deltaTime);
	}
}
