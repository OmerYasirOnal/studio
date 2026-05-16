import { useMetaStore } from '@/state/metaStore';

export default function Profile({ onClose }: { onClose: () => void }) {
  const { totalRuns, bestKills, longestRun, totalGold, totalXpEarned } = useMetaStore();
  const min = Math.floor(longestRun / 60);
  const sec = Math.floor(longestRun % 60)
    .toString()
    .padStart(2, '0');

  return (
    <div className="overlay overlay--blocking" onClick={onClose}>
      <div className="card" style={{ minWidth: 320 }} onClick={(e) => e.stopPropagation()}>
        <h2 className="title">Profile</h2>
        <div className="endrun-stats">
          <div>
            <div className="endrun-stat__label">Total Runs</div>
            <div className="endrun-stat__value">{totalRuns}</div>
          </div>
          <div>
            <div className="endrun-stat__label">Best Kills</div>
            <div className="endrun-stat__value">{bestKills}</div>
          </div>
          <div>
            <div className="endrun-stat__label">Longest Run</div>
            <div className="endrun-stat__value">
              {min}:{sec}
            </div>
          </div>
          <div>
            <div className="endrun-stat__label">Total Gold</div>
            <div className="endrun-stat__value">{totalGold}</div>
          </div>
        </div>
        <div className="endrun-stat__label" style={{ marginTop: 8 }}>
          Total XP earned: {totalXpEarned}
        </div>
        <div style={{ marginTop: 16 }}>
          <button className="btn btn--ghost" onClick={onClose} style={{ width: '100%' }}>
            Close
          </button>
        </div>
      </div>
    </div>
  );
}
