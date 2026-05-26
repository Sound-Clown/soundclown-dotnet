globalThis.musicPlayer = {
  audio: new Audio(),
  _dotnetRef: null,
  _currentUrl: null,

  init() {
    this.audio.ontimeupdate = () => {
      const cur = Math.floor(this.audio.currentTime);
      const dur = Number.isNaN(this.audio.duration) ? 0 : Math.floor(this.audio.duration);
      if (this._dotnetRef) {
        this._dotnetRef.invokeMethodAsync('OnProgress', cur, dur).catch(() => {});
      }
    };
    this.audio.onended = () => {
      if (this._dotnetRef) {
        this._dotnetRef.invokeVoidAsync('OnSongEnded').catch(() => {});
      }
    };
  },

  setDotNetRef(dotnetRef) {
    this._dotnetRef = dotnetRef;
  },

  play(url) {
    if (this._currentUrl !== url) {
      this._currentUrl = url;
      this.audio.src = url;
    }
    this.audio.play();
  },
  pause() { this.audio.pause(); },
  resume() { this.audio.play(); },
  seek(time) { this.audio.currentTime = time; },
  setVolume(v) { this.audio.volume = Math.max(0, Math.min(1, Number(v))); },
  getCurrentTime() { return Math.floor(this.audio.currentTime); },
  getDuration() { return Number.isNaN(this.audio.duration) ? 0 : Math.floor(this.audio.duration); }
};

globalThis.schedulePlayCount = (dotnetRef, songId) => {
  setTimeout(() => dotnetRef.invokeVoidAsync('OnPlayThreshold', songId).catch(() => {}), 30000);
};
