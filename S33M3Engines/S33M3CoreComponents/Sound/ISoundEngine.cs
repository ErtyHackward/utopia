﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Sound
{
    public interface ISoundEngine
    {
        public void PlaySound(string soundfile, float volume = 1);
    }
}
