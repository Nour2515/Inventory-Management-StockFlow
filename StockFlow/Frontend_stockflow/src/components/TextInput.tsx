type TextInputProps = {
  label: string;
  name: string;
  type?: string;
  required?: boolean;
};

export function TextInput({ label, name, type = "text", required }: TextInputProps) {
  return (
    <label>
      {label}
      <input name={name} type={type} required={required} />
    </label>
  );
}
