using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S33M3CoreComponents.Maths;
using SharpDX;
using Utopia.Shared;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Tests
{
    [TestClass]
    public class OtherTests
    {
        [TestMethod]
        public void CheckCountAtLeast()
        {
            var list = new List<int>();

            for (int i = 0; i < 100; i++)
            {
                list.Add(i);
            }

            Assert.IsTrue(list.CountAtLeast(0));
            Assert.IsTrue(list.CountAtLeast(10));
            Assert.IsTrue(list.CountAtLeast(100));
            Assert.IsFalse(list.CountAtLeast(110));
        }

        [TestMethod]
        public void ShadowBiasTest()
        {
            var sunVectorMorning = new Vector3(0.1f, -0.898f, 0);
            var sunVectorDay = new Vector3(-0.001f, -0.998f, 0);
            var sunVectorEvening = new Vector3(-0.73996f, -0.67265f, 0);
            var sunVectorEvening2 = new Vector3(-0.93996f, -0.17265f, 0);
            var topNormal = new Vector3(0,1,0);
            var bottomNormal = new Vector3(0, -1, 0);
            var leftNormal = new Vector3(-1, 0, 0);
            var rightNormal = new Vector3(1, 0, 0);
            var frontNormal = new Vector3(0, 0, 1);
            var backNormal = new Vector3(0, 0, -1);


            Trace.WriteLine("Top " + Vector3.Dot(topNormal, sunVectorMorning));
            Trace.WriteLine("Bottom " + Vector3.Dot(bottomNormal, sunVectorMorning));
            Trace.WriteLine("Left " + Vector3.Dot(leftNormal, sunVectorMorning));
            Trace.WriteLine("Right " + Vector3.Dot(rightNormal, sunVectorMorning));
            Trace.WriteLine("Front " + Vector3.Dot(frontNormal, sunVectorMorning));
            Trace.WriteLine("Back " + Vector3.Dot(backNormal, sunVectorMorning));
            Trace.WriteLine("---");
            Trace.WriteLine("Top " + Vector3.Dot(topNormal, sunVectorDay));
            Trace.WriteLine("Bottom " + Vector3.Dot(bottomNormal, sunVectorDay));
            Trace.WriteLine("Left " + Vector3.Dot(leftNormal, sunVectorDay));
            Trace.WriteLine("Right " + Vector3.Dot(rightNormal, sunVectorDay));
            Trace.WriteLine("Front " + Vector3.Dot(frontNormal, sunVectorDay));
            Trace.WriteLine("Back " + Vector3.Dot(backNormal, sunVectorDay));

            //float3 norm = float3(normalsX[facetype], normalsY[facetype], normalsZ[facetype]);

            //float cosTheta = dot(norm, SunVector);
            //float bias = tan(acos(cosTheta)) * SHADOW_EPSILON;
            //output.Bias = clamp(abs(bias), 0.0002f, 0.006);

            Trace.WriteLine(string.Format("{0} = {1}", sunVectorDay, GetBias(topNormal, sunVectorDay)));
            Trace.WriteLine(string.Format("{0} = {1}", sunVectorEvening, GetBias(topNormal, sunVectorEvening)));
            Trace.WriteLine(string.Format("{0} = {1}", sunVectorEvening2, GetBias(topNormal, sunVectorEvening2)));


        }

        private float GetBias(Vector3 normal, Vector3 sunVector)
        {
            var cosTheta = Math.Abs(Vector3.Dot(sunVector, normal));
            var bias = (float)Math.Tan(Math.Acos(cosTheta)) * 0.0002f;
            Trace.WriteLine(bias);
            return MathHelper.Clamp(bias, 0.0002f, 0.006f);
        }
    }
}
