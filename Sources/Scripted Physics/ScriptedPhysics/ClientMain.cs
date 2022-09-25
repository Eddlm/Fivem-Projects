using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;

namespace MyResource.ScriptedPhysics
{
    public class ClientMain : BaseScript
    {


        public ClientMain()
        {


        }

        [Tick]
        public Task OnTick()
        {

            return Task.FromResult(0);
        }


    }
}