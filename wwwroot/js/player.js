globalThis.musicPlayer = {
  audio: new Audio(),
  play(url) {
    this.audio.src = url;
    this.audio.play();
  },
  pause() { this.audio.pause(); },
  seek(time) { this.audio.currentTime = time; },
  getCurrentTime() { return this.audio.currentTime; },
  getDuration() { return Number.isNaN(this.audio.duration) ? 0 : this.audio.duration; },
  setVolume(vol) { this.audio.volume = vol; },
  getVolume() { return this.audio.volume; },
  setOnEnded(dotnetRef) {
    this.audio.onended = () => dotnetRef.invokeVoidAsync("OnSongEnded");
  },
  setOnTimeUpdate(dotnetRef) {
    this.audio.ontimeupdate = () =>
      dotnetRef.invokeVoidAsync("OnTimeUpdate", Math.floor(this.audio.currentTime));
  },
  setOnCanPlay(dotnetRef) {
    this.audio.oncanplay = () => dotnetRef.invokeVoidAsync("OnCanPlay", Math.floor(this.audio.duration));
  }
};

globalThis.schedulePlayCount = (dotnetRef, songId) => {
  setTimeout(() => dotnetRef.invokeVoidAsync("OnPlayThreshold", songId), 30000);
};
