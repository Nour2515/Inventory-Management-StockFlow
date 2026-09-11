export function HomePage() {
  return (
    <main>
      <h1>StockFlow</h1>
      <p>
        A simple connected website for using the existing inventory backend.
      </p>
      <section className="panel">
        <h2>Basic flow</h2>
        <ol>
          <li>Login or register.</li>
          <li>Review products and inventory.</li>
          <li>Create an order from the products dropdown.</li>
          <li>Create or manage reservations from your orders.</li>
          <li>Use SignalR to watch inventory updates and low stock alerts.</li>
        </ol>
      </section>
    </main>
  );
}
