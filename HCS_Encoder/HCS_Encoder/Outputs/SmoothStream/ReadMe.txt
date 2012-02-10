
HCS -> SmoothStream bridge
==========================

The HCS system was designed to output MPEG-TS files and playlists
to be compatible with Apple's HTTP Live Streaming specification.

The components in the "SmoothStream" folder transform the MPEG-TS
files into MP4f files for SmoothStreaming.

These components should work with both Off-line (VOD) and live
scenarios.

Where to start?
---------------

To get a plain MP4f file from a set of TS files, start with 'ChunkTransformer'
(this is similar to 'HTTP_Live_Streaming/UploadManager.cs')

TODO:
	- An upload manager for live SmoothStreaming
	- A manifest writer for VOD SmoothStreaming.

Why Bridge?
-----------

The HTTP Live Streaming solution requires far less server infrastructure
than SmoothStreaming, but requires more file management.

The MPEG-TS format is very close to the raw Elementary Streams, and is a
good starting point for muxing into other formats. The class
'MpegTS_Demux' can be used to get ES frames for both audio and video
which are used by the bridge.