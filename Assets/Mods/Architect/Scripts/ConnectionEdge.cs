using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates;
using RoR2.CharacterAI;
using RoR2.Orbs;

namespace Architect
{
    class ConnectionEdge : MonoBehaviour
    {
        ////public static Dictionary<CharacterBody, List<CharacterBody>> Sources = new Dictionary<CharacterBody, List<CharacterBody>>();
        ////public static Dictionary<CharacterBody, List<CharacterBody>> Sinks = new Dictionary<CharacterBody, List<CharacterBody>>();
        ////public static Dictionary<CharacterBody, List<CharacterBody>> BlueSources = new Dictionary<CharacterBody, List<CharacterBody>>();
        ////public static Dictionary<CharacterBody, List<CharacterBody>> BlueSinks = new Dictionary<CharacterBody, List<CharacterBody>>();
        ////public static Dictionary<CharacterBody, List<CharacterBody>> GreenSources = new Dictionary<CharacterBody, List<CharacterBody>>();
        ////public static Dictionary<CharacterBody, List<CharacterBody>> GreenSinks = new Dictionary<CharacterBody, List<CharacterBody>>();
        ////public static Dictionary<CharacterBody, List<CharacterBody>> RedSources = new Dictionary<CharacterBody, List<CharacterBody>>();
        ////public static Dictionary<CharacterBody, List<CharacterBody>> RedSinks = new Dictionary<CharacterBody, List<CharacterBody>>();
        ////public static Dictionary<OutputColor, Dictionary<CharacterBody, List<CharacterBody>>> ColorSources = new Dictionary<OutputColor, Dictionary<CharacterBody, List<CharacterBody>>>();
        ////public static Dictionary<OutputColor, Dictionary<CharacterBody, List<CharacterBody>>> ColorSinks = new Dictionary<OutputColor, Dictionary<CharacterBody, List<CharacterBody>>>();
        ////static Dictionary<(CharacterBody, CharacterBody), ConnectionEdge> edges = new Dictionary<(CharacterBody, CharacterBody), ConnectionEdge>();
        ////static List<HealthComponent> emptyBounced = new List<HealthComponent>();

        ////static Dictionary<CharacterBody, List<NodeData>> effects;

        ////public NodeInfo SourceInfo;
        ////public NodeInfo SinkInfo;
        ////public OutputColor Color;

        ////private const float actInterval = 1f;
        ////private float actTimer = 0.0f;
        ////private const float fullBarrierIntervals = 60f;

        ////public static bool canCalculate;
        //////public static IEnumerable<List<NodeData>> activeEffects;
        ////public static NodeData activeData;

        ////private bool init;

        //public void Init(NodeInfo srcInfo, NodeInfo snkInfo)
        //{
        //    //SourceInfo = srcInfo;
        //    //SinkInfo = snkInfo;

        //    //AddConnection(SourceInfo.body, SinkInfo.body);

        //    //edges.Add((SourceInfo.body, SinkInfo.body), this);

        //    //init = true;
            
        //    //if (!Util.HasEffectiveAuthority(gameObject))
        //    //    return;

        //    //if (!SourceInfo.body.HasBuff(BuffIndex.Cloak))
        //    //    SourceInfo.body.AddBuff(BuffIndex.Cloak);
        //    //SourceInfo.health.godMode = true;
        //    //SourceInfo.body.masterObject.GetComponent<BaseAI>().enabled = false;
        //}

        //public void FixedUpdate()
        //{
        //    //if (Util.HasEffectiveAuthority(GetComponent<NetworkIdentity>()))
        //    //{
        //    //    if (!SourceInfo.body || !SinkInfo.body || !ConnectionExists(SourceInfo.body, SinkInfo.body))
        //    //    {
        //    //        NetworkServer.Destroy(gameObject);
        //    //        return;
        //    //    }
        //    //    actTimer += Time.fixedDeltaTime;
        //    //    if (actTimer >= actInterval)
        //    //    {
        //    //        actTimer -= actInterval;

        //    //        if (Negative)
        //    //        {
        //    //            var info = new DamageInfo()
        //    //            {
        //    //                damage = SourceInfo.body.damage,
        //    //                crit = false,
        //    //                inflictor = gameObject,
        //    //                attacker = SourceInfo.body.gameObject,
        //    //                position = SinkInfo.body.transform.position,
        //    //                force = Vector3.zero,
        //    //                rejected = false,
        //    //                procChainMask = new ProcChainMask() { mask = 0 },
        //    //                procCoefficient = 0.0f,
        //    //                damageType = DamageType.Generic,
        //    //                damageColorIndex = DamageColorIndex.Default,
        //    //                dotIndex = DotController.DotIndex.None
        //    //            };
        //    //            SourceInfo.health.TakeDamage(info);
        //    //        }
        //    //        else
        //    //        { 
        //    //            GuardTarget.healthComponent.AddBarrier((SourceInfo.body.maxHealth + SourceInfo.body.maxShield) / fullBarrierIntervals);

        //    //            emptyBounced.Clear();
        //    //            LightningOrb lightningOrb = new LightningOrb
        //    //            {
        //    //                origin = GuardTarget.aimOrigin,
        //    //                damageValue = GuardTarget.damage,
        //    //                isCrit = false,
        //    //                bouncesRemaining = 0,
        //    //                teamIndex = GuardTarget.teamComponent.teamIndex,
        //    //                attacker = SourceInfo.body.gameObject,
        //    //                procCoefficient = 0.0f,
        //    //                bouncedObjects = emptyBounced,
        //    //                lightningType = LightningOrb.LightningType.Tesla,
        //    //                damageColorIndex = DamageColorIndex.Item,
        //    //                range = 35f
        //    //            };
        //    //            HurtBox hurtBox = lightningOrb.PickNextTarget(GuardTarget.transform.position);
        //    //            if (hurtBox)
        //    //            {
        //    //                lightningOrb.target = hurtBox;
        //    //                OrbManager.instance.AddOrb(lightningOrb);
        //    //            }
        //    //        }
        //    //    }
        //    //}
        //}

        //public void OnDestroy()
        //{
        //    //if (!init)
        //    //    return;
        //    //RemoveConnection(SourceInfo.body, SinkInfo.body);
        //    //edges.Remove((SourceInfo.body, SinkInfo.body));

        //    //if (!Util.HasEffectiveAuthority(gameObject))
        //    //    return;

        //    //if (!SinkInfo.body)
        //    //{
        //    //    SourceInfo.health.godMode = false;
        //    //    SourceInfo.health.Suicide();
        //    //}
        //}

        //public enum Direction
        //{
        //    Incoming,
        //    Outgoing
        //}

        //public static bool ConnectionExists(CharacterBody source, CharacterBody sink)
        //{
        //    if (!Sources.TryGetValue(sink, out var srcList))
        //        return false;
        //    else if (!srcList.Contains(source))
        //        return false;
        //    else if (!Sinks.TryGetValue(source, out var snkList))
        //        return false;
        //    else if (!snkList.Contains(sink))
        //        return false;
        //    return true;
        //}

        //public static int SourceConnectionCount(CharacterBody body)
        //{
        //    return Sources[body].Count;
        //}

        //public static int SinkColorConnectionCount(CharacterBody body, OutputColor color)
        //{
        //    return ColorSinks[color][body].Count;
        //}

        //public static ConnectionEdge GetConnection(CharacterBody source, CharacterBody sink)
        //{
        //    return edges[(source, sink)];
        //}

        //public static void AddConnection(CharacterBody source, CharacterBody sink)
        //{
        //    if (!Sources.ContainsKey(sink))
        //        Sources[sink] = new List<CharacterBody>();
        //    Sources[sink].Add(source);
        //    if (!Sinks.ContainsKey(source))
        //        Sinks[source] = new List<CharacterBody>();
        //    Sinks[source].Add(sink);
        //}

        //public static void RemoveConnection(CharacterBody source, CharacterBody sink)
        //{
        //    Sources[sink].Remove(source);
        //    Sinks[source].Remove(sink);

        //    if (Sources[sink].Count == 0)
        //        Sources.Remove(sink);
        //    if (Sinks[source].Count == 0)
        //        Sinks.Remove(source);
        //}

        //public static void RemoveColorConnection(CharacterBody source, CharacterBody sink, OutputColor color)
        //{
        //    Sources[sink].Remove(source);
        //    ColorSinks[color][source].Remove(sink);

        //    if (Sources[sink].Count == 0)
        //        Sources.Remove(sink);
        //    if (ColorSinks[color][source].Count == 0)
        //        ColorSinks[color].Remove(source);
        //}

        //public static void RemoveFirstSourceConnection(CharacterBody body)
        //{
        //    Sources[body].RemoveAt(0);
        //    if (Sources[body].Count == 0)
        //        Sources.Remove(body);
        //}

        ////public static void RecursiveGatherEdgeSources(CharacterBody body, ref List<ConnectionEdge> list)
        ////{
        ////    if (Sources.TryGetValue(body, out var gatherSources))
        ////        foreach (var source in gatherSources)
        ////        {
        ////            list.Add(GetConnection(source, body));
        ////            RecursiveGatherEdgeSources(source, ref list);
        ////        }
        ////}

        ////public static CharacterBody RecursiveGatherSinkHead(CharacterBody body)
        ////{
        ////    if (Sinks.TryGetValue(body, out var sinkList))
        ////        if (sinkList.Count > 0)
        ////            return RecursiveGatherSinkHead(sinkList[0]);
        ////    return body;
        ////}

        //public class NodeInfo
        //{
        //    public CharacterBody body;
        //    public HealthComponent health;
        //}
    }
}
