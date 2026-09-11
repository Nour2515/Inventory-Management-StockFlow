type ResultPanelProps = {
  result: unknown;
  error: string;
  loading: boolean;
};

export function ResultPanel({ result, error, loading }: ResultPanelProps) {
  return (
    <section className="result-panel">
      <h3>Result</h3>
      {loading && <p>Loading...</p>}
      {error && <pre className="error">{error}</pre>}
      {!loading && !error && result !== null && (
        <pre>{JSON.stringify(result, null, 2)}</pre>
      )}
      {!loading && !error && result === null && <p>No request sent yet.</p>}
    </section>
  );
}
