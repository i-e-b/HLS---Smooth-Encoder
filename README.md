An FFmpeg-based live and offline encoder that outputs HLS archives or publishes to IIS Live Smooth Streaming endpoints.

Can capture video from DirectShow cameras and audio equipment.

Can use a plugin framework for audio and video pre-processing (includes a tone-detector and a watermark plugin).

Uses the FFmpegControl library for encoding and decoding ( https://github.com/i-e-b/FFmpegControl )
The `.ts` files this outputs don't have correct CRCs yet. To play with VLC, open the 'Preferences' window,
select "Show settings: All", find `Input / Codecs` -> `Demuxers` and set the demux module to `Avformat demuxer`
If you use the command line interface, use `--demux avformat`

