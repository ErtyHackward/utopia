using S33M3DXEngine.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;
using SharpDX;
using SharpDX.X3DAudio;
using SharpDX.XAudio2;

namespace S33M3CoreComponents.Sound
{
    public interface ISoundEngine : IDisposable
    {
        /// <summary>
        /// Global volume for music [0;1]
        /// </summary>
        float GlobalMusicVolume { get; set; }

        /// <summary>
        /// Global volume for FX [0;1]
        /// </summary>
        float GlobalFXVolume { get; set; }

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
        float GeneralSoundVolume { get; set; }

        /// <summary>
        /// Acces of the Listener object
        /// </summary>
        Listener Listener { get; }

        /// <summary>
        /// Acces to the Detail of the hardware device being played against
        /// </summary>
        DeviceDetails DeviceDetail { get; }

        /// <summary>
        /// Access to the 3D audio engine
        /// </summary>
        X3DAudio X3DAudio { get; }

        /// <summary>
        /// Access to wrapped root XAudio Engine
        /// </summary>
        XAudio2 Xaudio2 { get; }

        /// <summary>
        /// Return custom channel mapping if existing
        /// </summary>
        /// <param name="inputChannelNbr">Qt of input sound channels</param>
        /// <param name="outputChannelNbr">Qt of speakers</param>
        /// <param name="mapping">The mapping factors</param>
        /// <returns>true/false following the succes of the retrieval</returns>
        bool GetCustomChannelMapping(int inputChannelNbr, int outputChannelNbr, out float[] mapping);

        /// <summary>
        /// Remove a specific mapping
        /// </summary>
        /// <param name="inputChannelNbr">Qt of input sound channels</param>
        /// <param name="outputChannelNbr">Qt of speakers</param>
        /// <returns>true/false following the succes of the operation</returns>
        bool RemoveCustomChannelMapping(int inputChannelNbr, int outputChannelNbr);

        /// <summary>
        /// Add a new channel mapping template
        /// </summary>
        /// <param name="inputChannelNbr">Qt of input sound channels</param>
        /// <param name="outputChannelNbr">Qt of speakers</param>
        /// <param name="mapping">The mapping configuration</param>
        /// <returns>true/false following the succes of the operation</returns>
        bool AddCustomChannelMapping(int inputChannelNbr, int outputChannelNbr, float[] mapping);

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
        /// <param name="soundPower">The maximum distance at wich the sound can be propagated : its "power", value in World unit</param>
        /// <returns>The soundDataSource object</returns>
        ISoundDataSource AddSoundSourceFromFile(string FilePath, string soundAlias, SourceCategory Category, bool? streamedSound = null, float soundPower = 16, int priority = 0);

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
        /// <param name="priority">if > -1, if all channels are playing, the oldest sound with a priority below this value will be forced to stop to give place to execute this command</param>
        /// <returns>The voice currently playing the sound</returns>
        ISoundVoice StartPlay2D(string FilePath, string soundAlias, SourceCategory Category = SourceCategory.FX, bool playLooped = false, uint fadeIn = 0, uint minDefferedStart = 0, uint maxDefferedStart = 0, int priority = 0);

        /// <summary>
        /// Start playing a sound in 2D mode
        /// </summary>
        /// <param name="FilePath">Path to the sound, in case the sound datasource was not created, will only be used if the alias is unknown</param>
        /// <param name="soundAlias">Sound Alias</param>
        /// <param name="playLooped">Keep on playing sound when finished</param>
        /// <param name="priority">if > -1, if all channels are playing, the oldest sound with a priority below this value will be forced to stop to give place to execute this command</param>
        /// <returns>The voice currently playing the sound</returns>
        ISoundVoice StartPlay2D(string soundAlias, float volume, SourceCategory Category = SourceCategory.FX, bool playLooped = false, uint fadeIn = 0, uint minDefferedStart = 0, uint maxDefferedStart = 0);


        /// <summary>
        /// Start playing a sound in 2D mode
        /// </summary>
        /// <param name="soundAlias">Sound Alias</param>
        /// <param name="playLooped">Keep on playing sound when finished</param>
        /// <param name="priority">if > -1, if all channels are playing, the oldest sound with a priority below this value will be forced to stop to give place to execute this command</param>
        /// <returns>The voice currently playing the sound</returns>
        ISoundVoice StartPlay2D(string soundAlias, SourceCategory Category = SourceCategory.FX, bool playLooped = false, uint fadeIn = 0, uint minDefferedStart = 0, uint maxDefferedStart = 0);
        /// <summary>
        /// Start playing a sound in 2D mode
        /// </summary>
        /// <param name="soundSource">The sound source to use for playing</param>
        /// <param name="playLooped">Keep on playing sound when finished</param>
        /// <param name="priority">if > -1, if all channels are playing, the oldest sound with a priority below this value will be forced to stop to give place to execute this command</param>
        /// <returns>The voice currently playing the sound</returns>
        ISoundVoice StartPlay2D(ISoundDataSource soundSource, bool playLooped = false, uint fadeIn = 0, uint minDefferedStart = 0, uint maxDefferedStart = 0);

        /// <summary>
        /// Start playing a sound in 2D mode
        /// </summary>
        /// <param name="soundSource">The sound source to use for playing</param>
        /// <param name="playLooped">Keep on playing sound when finished</param>
        /// <param name="priority">if > -1, if all channels are playing, the oldest sound with a priority below this value will be forced to stop to give place to execute this command</param>
        /// <returns>The voice currently playing the sound</returns>
        ISoundVoice StartPlay2D(ISoundDataSource soundSource, float volume, bool playLooped = false, uint fadeIn = 0, uint minDefferedStart = 0, uint maxDefferedStart = 0);


        ISoundVoice StartPlay2D(ISoundDataSourceBase soundSource, bool playLooped = false, uint fadeIn = 0, uint minDefferedStart = 0, uint maxDefferedStart = 0);
        

        /// <summary>
        /// Start Playing a sound in 3D Mode
        /// </summary>
        /// <param name="FilePath">Path to the sound, in case the sound datasource was not created, will only be used if the alias is unknown</param>
        /// <param name="soundAlia">Sound Alias</param>
        /// <param name="position">Sound world position</param>
        /// <param name="playLooped">Keep on playing sound when finished</param>
        /// <param name="priority">if > -1, if all channels are playing, the oldest sound with a priority below this value will be forced to stop to give place to execute this command</param>
        /// <returns>The voice currently playing the soun</returns>
        ISoundVoice StartPlay3D(string FilePath, string soundAlias, Vector3 position, SourceCategory Category = SourceCategory.FX, bool playLooped = false, uint minDefferedStart = 0, uint maxDefferedStart = 0, int priority = 0);

        /// <summary>
        /// Start Playing a sound in 3D Mode
        /// </summary>
        /// <param name="FilePath">Path to the sound, in case the sound datasource was not created, will only be used if the alias is unknown</param>
        /// <param name="soundAlia">Sound Alias</param>
        /// <param name="position">Sound world position</param>
        /// <param name="playLooped">Keep on playing sound when finished</param>
        /// <param name="priority">if > -1, if all channels are playing, the oldest sound with a priority below this value will be forced to stop to give place to execute this command</param>
        /// <returns>The voice currently playing the soun</returns>
        ISoundVoice StartPlay3D(string soundAlias, float volume, Vector3 position, SourceCategory Category = SourceCategory.FX, bool playLooped = false, uint minDefferedStart = 0, uint maxDefferedStart = 0);

        /// <summary>
        /// Start Playing a sound in 3D Mode
        /// </summary>
        /// <param name="soundAlia">Sound Alias</param>
        /// <param name="position">Sound world position</param>
        /// <param name="playLooped">Keep on playing sound when finished</param>
        /// <param name="priority">if > -1, if all channels are playing, the oldest sound with a priority below this value will be forced to stop to give place to execute this command</param>
        /// <returns>The voice currently playing the soun</returns>
        ISoundVoice StartPlay3D(string soundAlias, Vector3 position, SourceCategory Category = SourceCategory.FX, bool playLooped = false, uint minDefferedStart = 0, uint maxDefferedStart = 0);
        /// <summary>
        /// Start Playing a sound in 3D Mode
        /// </summary>
        /// <param name="soundSource">The sound source to use for playing</param>
        /// <param name="position">Sound world position</param>
        /// <param name="playLooped">Keep on playing sound when finished</param>
        /// /// <param name="priority">if > -1, if all channels are playing, the oldest sound with a priority below this value will be forced to stop to give place to execute this command</param>
        /// <returns>The voice currently playing the soun</returns>
        ISoundVoice StartPlay3D(ISoundDataSource soundSource, Vector3 position, bool playLooped = false, uint minDefferedStart = 0, uint maxDefferedStart = 0);

        /// <summary>
        /// Start Playing a sound in 3D Mode
        /// </summary>
        /// <param name="soundSource">The sound source to use for playing</param>
        /// <param name="position">Sound world position</param>
        /// <param name="playLooped">Keep on playing sound when finished</param>
        /// <param name="priority">if > -1, if all channels are playing, the oldest sound with a priority below this value will be forced to stop to give place to execute this command</param>
        /// <returns>The voice currently playing the soun</returns>
        ISoundVoice StartPlay3D(ISoundDataSource soundSource, Vector3 position, float volume, bool playLooped = false, uint minDefferedStart = 0, uint maxDefferedStart = 0);

        ISoundVoice StartPlay3D(ISoundDataSourceBase soundSource, Vector3 position, bool playLooped = false, uint minDefferedStart = 0, uint maxDefferedStart = 0);

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

        /// <summary>
       /// Queuing voice that needs to be look at (For looping, or fading reasons)
       /// </summary>
        void AddSoundWatching(ISoundVoice soundVoice);
    }
}
