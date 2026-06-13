globalThis.musicPlayer = {
  audio: new Audio(),
  _dotnetRef: null,
  _currentUrl: null,
  _playCountTimer: null,

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
        this._dotnetRef.invokeMethodAsync('OnSongEnded').catch(() => {});
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

// Giữ duy nhất 1 timer đếm lượt nghe: đổi bài sẽ hủy timer cũ để bài bị bỏ
// dở trước 30s không bị đếm. Khi timer kích hoạt, audio phải còn đang phát —
// pause trước mốc 30s thì không tính là một lượt nghe.
globalThis.schedulePlayCount = (dotnetRef, songId) => {
  const player = globalThis.musicPlayer;
  if (player._playCountTimer) {
    clearTimeout(player._playCountTimer);
  }
  player._playCountTimer = setTimeout(() => {
    player._playCountTimer = null;
    if (!player.audio.paused) {
      dotnetRef.invokeMethodAsync('OnPlayThreshold', songId).catch(() => {});
    }
  }, 30000);
};
