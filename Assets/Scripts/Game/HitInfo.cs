using UnityEngine;
using HutongGames.PlayMaker;

public class HitInfo : MonoBehaviour {
    public const string hitMessage = "OnHitInfoPerform";

    public struct Data {
        public Vector3 srcPos;
        public Vector3 srcDir;
        public bool explosion;
    }

    public float force;
    public bool isExplosion;

    public float damage;
    public float radius;

    public LayerMask layers;

    /// <summary>
    /// Perform a hit, returns number of hits made.
    /// </summary>
    public int Perform(Vector3 pos, Vector3 dir, float distance) {
        Data sendInfo = new Data() { srcPos = pos, srcDir = dir, explosion = isExplosion };

        if(isExplosion) {
            Collider[] cols = Physics.OverlapSphere(pos, radius, layers);

            foreach(Collider col in cols) {
                Stat stat = col.GetComponent<Stat>();
                if(stat != null) {
                    stat.curHP -= damage;
                }

                if(col.rigidbody != null && !col.rigidbody.isKinematic) {
                    col.rigidbody.AddExplosionForce(force, pos, radius);
                }

                col.SendMessage(hitMessage, sendInfo, SendMessageOptions.DontRequireReceiver);
            }

            //TODO: hit effect on point? (particle, sound, etc)

            return cols.Length;
        }
        else {
            RaycastHit[] hits = Physics.SphereCastAll(pos, radius, dir, distance, layers);

            foreach(RaycastHit hit in hits) {
                Stat stat = hit.collider.GetComponent<Stat>();
                if(stat != null) {
                    stat.curHP -= damage;
                }

                if(hit.rigidbody != null && !hit.rigidbody.isKinematic) {
                    hit.rigidbody.AddForceAtPosition(dir * force, hit.point, ForceMode.Impulse);
                }

                hit.collider.SendMessage(hitMessage, sendInfo, SendMessageOptions.DontRequireReceiver);

                //TODO: hit effect on point? (particle, sound, etc)
            }

            return hits.Length;
        }
    }
}
