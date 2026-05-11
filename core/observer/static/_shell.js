// _shell.js — shared topbar (brand + active-game switcher + nav).
// Loaded before page-specific scripts. Exports `STUDIO` global.

(function () {
  const STUDIO = {
    base: "",
    activeGame: "",
    games: [],
    async loadGames() {
      const r = await fetch("/api/games");
      const j = await r.json();
      this.games = j.games || [];
      this.activeGame = (new URLSearchParams(location.search).get("game")) || j.active || (this.games[0] && this.games[0].slug) || "";
      return j;
    },
    renderTopbar(currentPage) {
      const pages = [
        ["index", "/static/index.html", "Overview"],
        ["phases", "/static/phases.html", "Phases"],
        ["decisions", "/static/decisions.html", "Decisions"],
        ["handoffs", "/static/handoffs.html", "Handoffs"],
        ["logs", "/static/logs.html", "Logs"],
      ];
      const navHtml = pages.map(([key, href, label]) => {
        const cls = key === currentPage ? "active" : "";
        return `<a class="${cls}" href="${href}?game=${encodeURIComponent(this.activeGame)}">${label}</a>`;
      }).join("");

      const optsHtml = this.games.length
        ? this.games.map(g => `<option value="${g.slug}"${g.slug === this.activeGame ? " selected" : ""}>${g.display_name}</option>`).join("")
        : `<option>(no games)</option>`;

      const header = document.createElement("header");
      header.className = "topbar";
      header.innerHTML = `
        <div class="brand">STUDIO ◎</div>
        <nav>${navHtml}</nav>
        <div class="right">
          <span style="color: var(--muted); font-size: 12px;">active game</span>
          <select id="game-switch">${optsHtml}</select>
          <span class="badge green" id="health-dot">●</span>
        </div>
      `;
      document.body.prepend(header);
      const sel = header.querySelector("#game-switch");
      sel.addEventListener("change", () => {
        const url = new URL(location.href);
        url.searchParams.set("game", sel.value);
        location.href = url.toString();
      });
      // Health poll.
      setInterval(async () => {
        try { const r = await fetch("/health"); const j = await r.json(); header.querySelector("#health-dot").className = j.ok ? "badge green" : "badge red"; }
        catch { header.querySelector("#health-dot").className = "badge red"; }
      }, 5000);
    },
    fmtTs(unix) {
      if (!unix) return "—";
      const d = new Date(unix * 1000);
      return d.toLocaleString();
    },
    fmtRelTs(unix) {
      if (!unix) return "—";
      const dt = (Date.now() / 1000) - unix;
      if (dt < 60) return `${Math.floor(dt)}s ago`;
      if (dt < 3600) return `${Math.floor(dt / 60)}m ago`;
      if (dt < 86400) return `${Math.floor(dt / 3600)}h ago`;
      return `${Math.floor(dt / 86400)}d ago`;
    },
    statusBadge(status) {
      if (!status) return `<span class="badge gray">—</span>`;
      const cls = { spawning: "yellow", working: "yellow", idle: "gray", blocked: "red", done: "green" }[status] || "gray";
      return `<span class="badge ${cls}">${status}</span>`;
    }
  };
  window.STUDIO = STUDIO;
})();
