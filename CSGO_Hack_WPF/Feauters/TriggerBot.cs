using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CSGO_Hack_WPF.Objects;
using CSGO_Hack_WPF.SDK;
using CSGO_Hack_WPF.Utils;

namespace CSGO_Hack_WPF.Feauters
{
    public enum TriggerType : int
    {
        inCross = 0,
        bone = 1,
        hitbox = 2
    }
    public class TriggerBot
    {
        #region Fields

        public bool AimOntarget;
        private long _triggerLastTarget;
        private long _triggerLastShot;
        private bool _trigger;
        private bool _triggerCamp;
        private bool _triggerZoom;
        private TriggerType _triggerType;
        public int TriggerDelayFirstRandomize;
        public int TriggerDelayShotsRandomize;
        private int _triggerDelayFirstShot;
        private int _triggerDelayShots;
        public Vector3 ViewAngles;
        private IEnumerable<Player> _validTargets;

        #endregion

        #region Methods

        public void Update()
        {
            if (!MiscUtils.ShouldUpdate())
                return;

            ReadSettings();

            if (!_trigger)
                return;

            //if (Core.KeyUtils.KeyIsDown(_triggerKey))
            //{
            ViewAngles = Engine.GetViewAngles();

            if (_triggerZoom)
                if (Core.LocalPlayerWeapon.ZoomLevel == 0)
                    return;

            if (_triggerCamp)
                if (Core.LocalPlayer.Velocity > 0)
                    return;

            RandomizeDelay();

            switch (_triggerType)
            {
                case TriggerType.inCross:
                    InCrossTriggerBot();
                    break;
                case TriggerType.bone:
                    BoneTriggerBot();
                    break;
                case TriggerType.hitbox:
                    HitBoxTriggerBot();
                    break;
            }
            //}
            //else
            //    AimOntarget = false;
        }

        private void HitBoxTriggerBot()
        {
            GetValidTargets();
            foreach (Player target in _validTargets)
            {
                if (target.Health > 0 & !target.IsDormant)
                {
                    Vector3 bBone = target.HeadPos + new Vector3(0, 0, 3);
                    Vector3 bottomHitboxHead = new Vector3(bBone.X - 2.54f, bBone.Y - 4.145f, bBone.Z - 7f);
                    Vector3 topHitboxHead = new Vector3(bBone.X + 2.54f, bBone.Y + 4.145f, bBone.Z + 3f);

                    Vector3 hBone = target.GetBone(3);
                    Vector3 bottomHitboxBody = new Vector3(hBone.X - 7f, hBone.Y - 5.5f, hBone.Z - 25f);
                    Vector3 topHitboxBody = new Vector3(hBone.X + 7f, hBone.Y + 5.5f, hBone.Z + 15f);

                    Vector3 viewDirection = TraceRay.AngleToDirection(ViewAngles);
                    TraceRay viewRay = new TraceRay(Core.LocalPlayer.Position + Core.LocalPlayer.VecView, viewDirection);
                    float distance = 0;

                    if (viewRay.Trace(bottomHitboxHead, topHitboxHead, ref distance) | viewRay.Trace(bottomHitboxBody, topHitboxBody, ref distance))
                    {
                        if (!CheckDelay())
                            return;

                        _triggerLastShot = DateTime.Now.Ticks;

                        Engine.ForceAttack(0, 12, 10);
                    }
                }

            }

        }

        public float GetNextEnemyToCrosshair(int bone, ref IntPtr pPointer)
        {
            float fov = 0;
            Vector3 pAngles = ViewAngles;

            int[] playerArray = new int[33];
            float[] angleArray = new float[33];


            for (int i = 1; i <= 32; i++)
            {
                Player player = new Player(Core.Objects.GetEntityPtr(i));

                Vector3 pAngle = player.GetBone(bone);
                pAngle = Core.LocalPlayer.Position.CalcAngle(pAngle);
                pAngle = pAngle.ClampAngle();
                float iDiff = Vector3.Distance(pAngle, pAngles);

                playerArray[i] = (int)player.BaseAddress;
                angleArray[i] = iDiff;
            }

            int closestPlayer = 0;
            float closestAngle = 360;

            for (int i = 1; i <= 32; i++)
            {
                Player player = new Player((IntPtr)playerArray[i]);
                float angle = angleArray[i];

                int curPlayerTeam = (int)player.Team;
                bool dormant = player.IsDormant;
                int health = player.Health;

                Vector3 pOriginVec = player.Position;
                pOriginVec.Z += 64;

                if (!(curPlayerTeam != (int)Core.LocalPlayer.Team & (!dormant) & health > 0 & angle < closestAngle))
                    continue;

                closestPlayer = (int)player.BaseAddress;
                closestAngle = angle;
                fov = angle;
            }
            pPointer = (IntPtr)closestPlayer;
            return fov;
        }

        private void InCrossTriggerBot()
        {
            BaseEntity target = Core.LocalPlayer.Target;
            if (target != null && !target.IsFriendly)
            {
                if (!AimOntarget)
                {
                    AimOntarget = true;
                    _triggerLastTarget = DateTime.Now.Ticks;
                }
                else
                {
                    if (!CheckDelay())
                        return;

                    _triggerLastShot = DateTime.Now.Ticks;

                    if (target.GunGameImmune)
                        return;

                    Engine.ForceAttack(0, 12, 10);
                }
            }
        }

        private void BoneTriggerBot()
        {
            GetValidTargets();
            foreach (Player validTarget in _validTargets)
            {
                Vector3 myView = Core.LocalPlayer.Position + Core.LocalPlayer.VecView;

                for (int i = 0; i < 81; i++)
                {
                    Vector3 aimView = validTarget.GetBone(i);
                    Vector3 dst = myView.CalcAngle(aimView);
                    dst = dst.NormalizeAngle();
                    var fov = MathUtils.Fov(ViewAngles, dst, Vector3.Distance(Core.LocalPlayer.Position, validTarget.Position));

                    if (!(fov <= 5))
                        continue;

                    if (!AimOntarget)
                    {
                        AimOntarget = true;
                        _triggerLastTarget = DateTime.Now.Ticks;
                    }
                    else
                    {
                        if (!CheckDelay())
                            return;

                        _triggerLastShot = DateTime.Now.Ticks;

                        Engine.ForceAttack(0, 12, 10);
                    }
                }
            }
        }

        private bool CheckDelay()
        {
            if (!(new TimeSpan(DateTime.Now.Ticks - _triggerLastTarget).TotalMilliseconds >= TriggerDelayFirstRandomize))
                return false;
            if (!(new TimeSpan(DateTime.Now.Ticks - _triggerLastShot).TotalMilliseconds >= TriggerDelayShotsRandomize))
                return false;

            return true;
        }

        private void GetValidTargets()
        {
            _validTargets = Core.Objects.Players.Where(p => p.IsAlive && !p.IsDormant && p.Id != Core.LocalPlayer.Id /*&& p.SeenBy(Core.LocalPlayer)*/);
            _validTargets = _validTargets.Where(p => !p.IsFriendly && !p.GunGameImmune);
        }

        private void RandomizeDelay()
        {
            TriggerDelayFirstRandomize = new Random().Next(_triggerDelayFirstShot, _triggerDelayFirstShot) + 1;
            TriggerDelayShotsRandomize = new Random().Next(_triggerDelayShots, _triggerDelayShots) + 1;
        }

        private void ReadSettings()
        {
            var config = MiscUtils.GetCurrentWeaponConfig().triggerConfig;
            _trigger = config.enabled;
            _triggerDelayFirstShot = config.delayFirstShot;
            _triggerDelayShots = config.delayShots;
            _triggerType = (TriggerType)config.type;
            _triggerCamp = config.camp;
            _triggerZoom = config.zoom;
        }
        #endregion
    }

}
