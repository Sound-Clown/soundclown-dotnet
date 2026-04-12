globalThis.musicPlayer = {
  audio: new Audio(),
  _dotnetRef: null,

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
    this.audio.src = url;
    this.audio.play();
  },
  pause() { this.audio.pause(); },
  resume() { this.audio.play(); },
  seek(time) { this.audio.currentTime = time; },
  getCurrentTime() { return Math.floor(this.audio.currentTime); },
  getDuration() { return Number.isNaN(this.audio.duration) ? 0 : Math.floor(this.audio.duration); }
};

globalThis.schedulePlayCount = (dotnetRef, songId) => {
  setTimeout(() => dotnetRef.invokeVoidAsync('OnPlayThreshold', songId).catch(() => {}), 30000);
};
