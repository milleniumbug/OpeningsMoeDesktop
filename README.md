OpeningsMoeWpfClient
====================

This is a "client" for the [openings.moe](http://openings.moe) webapp. It's bare bones in functionality, and it has several "issues", or design choices, whatever you may call them:

- the entire video is downloaded before playing. This is by design, because I'm not a fan of music being interrupted at random moments.
- downloaded videos are never deleted. This is also by design.
- WebM/VP9/Vorbis files aren't accepted by this implementation, so they're transcoded to a more primitive format, at the cost of quality. This means that a.) ffmpeg.exe is required to be in %PATH% b.) the video quality is waaaaaaay worse. This is not by design, but due to current implementation choices. Hopefully this will get better in the future.
- almost no features from the original client. I'll implement them if I feel like it in the future.
- automatic sound volume normalization is to be implemented.