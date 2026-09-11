type TextInputProps = {
  label: string;
  name: string;
  type?: string;
  required?: boolean;
  defaultValue?: string;
};

export function TextInput({
  label,
  name,
  type = "text",
  required,
  defaultValue,
}: TextInputProps) {
  return (
    <label>
      {label}
      <input name={name} type={type} required={required} defaultValue={defaultValue} />
    </label>
  );
}
