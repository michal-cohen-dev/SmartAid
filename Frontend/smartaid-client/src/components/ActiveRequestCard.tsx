
interface ActiveRequestCardProps {
  title: string;
  description: string;
  distance: string;
  status: string;
}

function ActiveRequestCard({
  title,
  description,
  distance,
  status
}: ActiveRequestCardProps) {
  return (
    <div>
      <h3>{title}</h3>
      <p>{description}</p>
      <p>מרחק: {distance}</p>
      <p>סטטוס: {status}</p>
    </div>
  );
}

export default ActiveRequestCard;
