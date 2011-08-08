using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Planets.Terran
{
    class Shadow
    {
        //Test for Shadow Creation ===> Big stability problem => Aborted
        // Needs to be called from the Terra.cs file

        //Matrix lightViewProjection;
        //Color4 whiteColor = new Color4(255, 255, 255, 255);
        //private bool SaveTexture = false;
        //private void DrawSolidFacesShadowMap()
        //{
        //    CreateLightViewProjectionMatrix2(out lightViewProjection);

        //    _terraEffect.CurrentTechnique = _terraEffect.Effect.GetTechniqueByName("CreateShadowMap");
        //    _terraEffect.LightViewProj = lightViewProjection;

        //    _shadowMap.Begin();

        //    TerraChunk chunk;
        //    Matrix worldFocus = Matrix.Identity;
        //    //Foreach faces type
        //    for (int chunkIndice = 0; chunkIndice < TerraWorld.ChunkGridSize * TerraWorld.ChunkGridSize; chunkIndice++)
        //    {
        //        chunk = World.Chunks[chunkIndice];

        //        if (chunk.Ready2Draw)
        //        {
        //            //Only checking Frustum with the faceID = 0
        //            chunk.FrustumCulled = !Game.ActivCamera.Frustum.Intersects(chunk.ChunkWorldBoundingBox);

        //            //if (!chunk.FrustumCulled)
        //            //{
        //                MathHelper.CenterOnFocus(ref chunk.World, ref worldFocus, ref Game.WorldFocus);
        //                _terraEffect.WorldVariable = worldFocus;
        //                _terraEffect.ApplyPass(0);
        //                chunk.DrawSolidFaces();
        //                //_chunkDrawByFrame++;
        //            //}
        //        }
        //    }

        //    _shadowMap.End();
        //    Game.D3dEngine.ResetRenderTargetsAndViewPort();

        //    if (!SaveTexture)
        //    {
        //        //Texture2D.SaveTextureToFile(Game.D3dEngine.Context, _shadowMapTex, ImageFileFormat.Dds, @"E:\1.dds");
        //        //SaveTexture = true;
        //    }
        //}

        ///// <summary>
        ///// Creates the WorldViewProjection matrix from the perspective of the 
        ///// light using the cameras bounding frustum to determine what is visible 
        ///// in the scene.
        ///// </summary>
        ///// <returns>The WorldViewProjection for the light</returns>
        //private void CreateLightViewProjectionMatrix(out Matrix lightProjection)
        //{
        //    // Matrix with that will rotate in points the direction of the light
        //    Vector3 lightDirection = -_skyDome.LightDirection;

        //    Matrix lightRotation = Matrix.LookAtRH(Vector3.Zero, -lightDirection, MVector3.Up);

        //    // Get the corners of the frustum
        //    Vector3[] frustumCorners = Game.ActivCamera.CloseFrustum.GetCorners();

        //    // Transform the positions of the corners into the direction of the light
        //    for (int i = 0; i < frustumCorners.Length; i++)
        //    {
        //        Vector3.TransformCoordinate(ref frustumCorners[i], ref lightRotation, out frustumCorners[i]);
        //    }

        //    // Find the smallest box around the points
        //    BoundingBox lightBox = BoundingBox.FromPoints(frustumCorners);

        //    Vector3 boxSize = lightBox.Maximum - lightBox.Minimum;
        //    Vector3 halfBoxSize = boxSize * 0.5f;

        //    // The position of the light should be in the center of the back
        //    // pannel of the box. 
        //    Vector3 lightPosition = lightBox.Minimum + halfBoxSize;
        //    lightPosition.Z = lightBox.Minimum.Z;

        //    // We need the position back in world coordinates so we transform 
        //    // the light position by the inverse of the lights rotation
        //    Matrix invertedMatrix = Matrix.Invert(lightRotation);
        //    MVector3.Transform(ref lightPosition, ref invertedMatrix, out lightPosition);

        //    lightPosition.X -= (float)Game.WorldFocus.FocusPoint.ActualValue.X;
        //    lightPosition.Y -= (float)Game.WorldFocus.FocusPoint.ActualValue.Y;
        //    lightPosition.Z -= (float)Game.WorldFocus.FocusPoint.ActualValue.Z;

        //    // Create the view matrix for the light
        //    Matrix lightView = Matrix.LookAtRH(lightPosition, lightPosition - lightDirection, MVector3.Up);

        //    // Create the projection matrix for the light
        //    // The projection is orthographic since we are using a directional light
        //    lightProjection = lightView * Matrix.OrthoRH(boxSize.X, boxSize.Y, -boxSize.Z, boxSize.Z);
        //}

        //public void CreateLightViewProjectionMatrix2(out Matrix lightProjection)
        //{
        //    float high = 128;
        //    float low = 0;
        //    Vector2 boxSize = new Vector2(300, 300);
        //    int texSize = 2048;
        //    Vector3 direction = _skyDome.LightDirection;
        //    // SOURCE: http://www.gamedev.n...topic_id=591684            

        //    // CREATE A BOX CENTERED AROUND THE pCAMERA POSITION:                      
        //    Vector3 min = - new Vector3(boxSize.X / 2, 0, boxSize.Y / 2);
        //    min.Y = low;
        //    Vector3 max = new Vector3(boxSize.X / 2, 0, boxSize.Y / 2);
        //    max.Y = high;
        //    BoundingBox boxWS = new BoundingBox(min, max);

        //    // CREATE A VIEW MATRIX OF THE SHADOW CAMERA            
        //    Vector3 shadowCamPos = Vector3.Zero;
        //    shadowCamPos.Y = high - low;
        //    Matrix shadowViewMatrix = Matrix.LookAtRH(shadowCamPos, (shadowCamPos + (direction * 300)), MVector3.Up);
        //    // TRANSFORM THE BOX INTO LIGHTSPACE COORDINATES:            
        //    Vector3[] cornersWS = boxWS.GetCorners();
        //    Vector3[] cornersLS = new Vector3[cornersWS.Length];
        //    Vector3.TransformCoordinate(cornersWS, ref shadowViewMatrix, cornersLS);
        //    BoundingBox box = BoundingBox.FromPoints(cornersLS);
        //    // CREATE PROJECTION MATRIX            
        //    Matrix shadowProjMatrix = Matrix.OrthoOffCenterRH(box.Minimum.X, box.Maximum.X, box.Minimum.Y, box.Maximum.Y, -box.Maximum.Z, -box.Minimum.Z);
        //    Matrix shadowViewProjMatrix = shadowViewMatrix * shadowProjMatrix;
        //    Vector3 shadowOrigin = Vector3.TransformCoordinate(Vector3.Zero, shadowViewProjMatrix);
        //    shadowOrigin *= (texSize / 2.0f);
        //    Vector2 roundedOrigin = new Vector2((float)Math.Round(shadowOrigin.X), (float)Math.Round(shadowOrigin.Y));
        //    Vector2 rounding = roundedOrigin - new Vector2(shadowOrigin.X, shadowOrigin.Y);
        //    rounding /= (texSize / 2.0f);
        //    Matrix roundMatrix = Matrix.Translation(new Vector3(rounding.X, rounding.Y, 0.0f));
        //    shadowViewProjMatrix *= roundMatrix;
        //    lightProjection = shadowViewProjMatrix;
        //}
    }
}
