using UnityEngine;

public class ThrownObject : MonoBehaviour
{
    public string enemyTag; 
    public ParticleSystem bloodParticleEffect; 

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }

            CreateBloodEffect(collision.contacts[0].point);
        }
        else if (collision.gameObject.CompareTag(enemyTag))
        {
            Animator targetAnimator = collision.gameObject.GetComponentInParent<Animator>();
            if (targetAnimator != null)
            {
                targetAnimator.SetBool("Death", true);
            }


            CreateBloodEffect(collision.contacts[0].point);
        }


        Destroy(this);
    }


    void CreateBloodEffect(Vector3 position)
    {

        if (bloodParticleEffect != null)
        {

            ParticleSystem blood = Instantiate(bloodParticleEffect, position, Quaternion.identity);
            blood.Play();
        }
    }
}
