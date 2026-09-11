import { getFieldErrors, getErrorMessage } from "../api/client";

type FormErrorProps = {
  error: unknown;
};

export function FormError({ error }: FormErrorProps) {
  if (!error) {
    return null;
  }

  const fieldErrors = getFieldErrors(error);
  const message = getErrorMessage(error);

  return (
    <div className="alert alert-error" role="alert">
      <p>{message}</p>
      {fieldErrors && (
        <ul className="field-error-list">
          {Object.entries(fieldErrors).flatMap(([field, messages]) =>
            messages.map((text) => (
              <li key={`${field}-${text}`}>
                <strong>{field}:</strong> {text}
              </li>
            )),
          )}
        </ul>
      )}
    </div>
  );
}
