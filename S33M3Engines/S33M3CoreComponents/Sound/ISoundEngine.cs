﻿using S33M3DXEngine.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;
using SharpDX;

namespace S33M3CoreComponents.Sound
{
    public interface ISoundEngine : IDisposable
    {
        /// <summary>
        /// A Name given to the sound engine
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The sound devices from the computers
        /// </summary>
        List<string> SoundDevices { get; }

        /// <summary>
        /// Default SoundVolume
        /// </summary>
        float DefaultSoundVolume { get; set; }
        /// <summary>
        /// Default Maximum Distance for playing a sound in 3d mode.
        /// </summary>
        float DefaultMaxDistance { get; set; }
        /// <summary>
        /// Default Minimum Distance for playing a sound in 3d mode.
        /// </summary>
        float DefaultMinDistance { get; set; }

        /// <summary>
        /// Set the Listener Position for 3D mode playing
        /// </summary>
        /// <param name="pos">The current position of the Listener</param>
        /// <param name="lookdir">The current looking direction of the listener</param>
        /// <param name="velPerSecond">The velocity m/s of the listener</param>
        /// <param name="upVector">The listener Up Vector</param>
        void SetListenerPosition(Vector3D pos, Vector3D lookDir, Vector3D velPerSecond, Vector3D upVector);
        /// <summary>
        /// Set the Listener Position for 3D mode playing
        /// </summary>
        /// <param name="pos">The current position of the Listener</param>
        /// <param name="lookdir">The current looking direction of the listener</param>
        void SetListenerPosition(Vector3D pos, Vector3D lookDir);
        /// <summary>
        /// Set the Listener Position for 3D mode playing
        /// </summary>
        /// <param name="pos">The current position of the Listener</param>
        /// <param name="lookdir">The current looking direction of the listener</param>
        /// <param name="velPerSecond">The velocity m/s of the listener</param>
        /// <param name="upVector">The listener Up Vector</param>
        void SetListenerPosition(Vector3 pos, Vector3 lookDir, Vector3 velPerSecond, Vector3 upVector);
        /// <summary>
        /// Set the Listener Position for 3D mode playing
        /// </summary>
        /// <param name="pos">The current position of the Listener</param>
        /// <param name="lookdir">The current looking direction of the listener</param>
        void SetListenerPosition(Vector3 pos, Vector3 lookDir);
        /// <summary>
        /// Start computing 3D effect for sound based on listener position vs object positions
        /// </summary>
        void Update3DSounds();


        /// <summary>
        /// Add a sound source, if not in stream mode, the sound will be buffered = Memory will be reserved for it.
        /// </summary>
        /// <param name="FilePath">The sound file Path</param>
        /// <param name="soundAlias">Alias given to the sound : An alias MUST be unic</param>
        /// <param name="streamedSound">Will the sound be played in stream mode or not (Stream = less heavy on memory needed)</param>
        /// <returns>The soundDataSource object</returns>
        ISoundDataSource AddSoundSourceFromFile(string FilePath, string soundAlias, bool streamedSound = false);
        /// <summary>
        /// Get a sound source via its alias
        /// </summary>
        /// <param name="soundAlias">The identification sound alias</param>
        /// <returns>The soundDataSource object</returns>
        ISoundDataSource GetSoundSource(string soundAlias);


        /// <summary>
        /// Start playing a sound in 2D mode
        /// </summary>
        /// <param name="FilePath">Path to the sound, in case the sound datasource was not created, will only be used if the alias is unknown</param>
        /// <param name="soundAlias">Sound Alias</param>
        /// <param name="playLooped">Keep on playing sound when finished</param>
        /// <returns>The voice currently playing the sound</returns>
        ISoundVoice StartPlay2D(string FilePath, string soundAlias, bool playLooped = false);
        /// <summary>
        /// Start playing a sound in 2D mode
        /// </summary>
        /// <param name="soundAlias">Sound Alias</param>
        /// <param name="playLooped">Keep on playing sound when finished</param>
        /// <returns>The voice currently playing the sound</returns>
        ISoundVoice StartPlay2D(string soundAlias, bool playLooped = false);
        /// <summary>
        /// Start playing a sound in 2D mode
        /// </summary>
        /// <param name="soundSource">The sound source to use for playing</param>
        /// <param name="playLooped">Keep on playing sound when finished</param>
        /// <returns>The voice currently playing the sound</returns>
        ISoundVoice StartPlay2D(ISoundDataSource soundSource, bool playLooped = false);

        /// <summary>
        /// Start Playing a sound in 3D Mode
        /// </summary>
        /// <param name="FilePath">Path to the sound, in case the sound datasource was not created, will only be used if the alias is unknown</param>
        /// <param name="soundAlia">Sound Alias</param>
        /// <param name="posX">Sound X world position</param>
        /// <param name="posY">Sound Y world position</param>
        /// <param name="posZ">Sound Z world position</param>
        /// <param name="playLooped">Keep on playing sound when finished</param>
        /// <returns>The voice currently playing the soun</returns>
        ISoundVoice StartPlay3D(string FilePath, string soundAlia, float posX, float posY, float posZ, bool playLooped = false);
        /// <summary>
        /// Start Playing a sound in 3D Mode
        /// </summary>
        /// <param name="FilePath">Path to the sound, in case the sound datasource was not created, will only be used if the alias is unknown</param>
        /// <param name="soundAlia">Sound Alias</param>
        /// <param name="position">Sound world position</param>
        /// <param name="playLooped">Keep on playing sound when finished</param>
        /// <returns>The voice currently playing the soun</returns>
        ISoundVoice StartPlay3D(string FilePath, string soundAlia, Vector3 position, bool playLooped = false);
        /// <summary>
        /// Start Playing a sound in 3D Mode
        /// </summary>
        /// <param name="FilePath">Path to the sound, in case the sound datasource was not created, will only be used if the alias is unknown</param>
        /// <param name="soundAlia">Sound Alias</param>
        /// <param name="position">Sound world position</param>
        /// <param name="playLooped">Keep on playing sound when finished</param>
        /// <returns>The voice currently playing the soun</returns>
        ISoundVoice StartPlay3D(string FilePath, string soundAlia, Vector3D position, bool playLooped = false);
        /// <summary>
        /// Start Playing a sound in 3D Mode
        /// </summary>
        /// <param name="soundSource">The sound source to use for playing</param>
        /// <param name="posX">Sound X world position</param>
        /// <param name="posY">Sound Y world position</param>
        /// <param name="posZ">Sound Z world position</param>
        /// <param name="playLooped">Keep on playing sound when finished</param>
        /// <returns>The voice currently playing the soun</returns>
        ISoundVoice StartPlay3D(ISoundDataSource soundSource, float posX, float posY, float posZ, bool playLooped = false);
        /// <summary>
        /// Start Playing a sound in 3D Mode
        /// </summary>
        /// <param name="soundSource">The sound source to use for playing</param>
        /// <param name="position">Sound world position</param>
        /// <param name="playLooped">Keep on playing sound when finished</param>
        /// <returns>The voice currently playing the soun</returns>
        ISoundVoice StartPlay3D(ISoundDataSource soundSource, Vector3 position, bool playLooped = false);
        /// <summary>
        /// Start Playing a sound in 3D Mode
        /// </summary>
        /// <param name="soundSource">The sound source to use for playing</param>
        /// <param name="position">Sound world position</param>
        /// <param name="playLooped">Keep on playing sound when finished</param>
        /// <returns>The voice currently playing the soun</returns>
        ISoundVoice StartPlay3D(ISoundDataSource soundSource, Vector3D position, bool playLooped = false);

        /// <summary>
        /// Check if a sound is currently being played
        /// </summary>
        /// <param name="soundAlias"></param>
        /// <returns></returns>
        bool IsCurrentlyPlaying(string soundAlias);

        /// <summary>
        /// Remove all buffered sound sources, will free up memory
        /// </summary>
        void RemoveAllSoundSources();
        /// <summary>
        /// Remove a specific sound source, will free up memory if was not in streaming mode
        /// </summary>
        /// <param name="soundAlias"></param>
        void RemoveSoundSource(string soundAlias);

        /// <summary>
        /// Stop playing all voices
        /// </summary>
        void StopAllSounds();
    }
}
