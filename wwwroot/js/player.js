window.musicPlayer = {
  audio: new Audio(),
  play(url) {
    this.audio.src = url;
    this.audio.play();
  },
  pause() { this.audio.pause(); },
  seek(time) { this.audio.currentTime = time; },
  getCurrentTime() { return this.audio.currentTime; },
  getDuration() { return isNaN(this.audio.duration) ? 0 : this.audio.duration; },
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

window.schedulePlayCount = (dotnetRef) => {
  setTimeout(() => dotnetRef.invokeVoidAsync("OnPlayThreshold"), 30000);
};
