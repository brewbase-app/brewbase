import { useState } from "react";

import "./MultiSelectInput.css";

function MultiSelectInput({
    options,
    value,
    onChange,
    allowCustom = false,
    customPlaceholder = "Inny...",
}) {
    const [customValue, setCustomValue] = useState("");

    const customOptions = value.filter(
        (item) => !options.includes(item)
    );

    const toggleOption = (option) => {
        if (value.includes(option)) {
            onChange(value.filter((item) => item !== option));
            return;
        }

        onChange([...value, option]);
    };

    const addCustomOption = () => {
        const trimmedValue = customValue.trim();

        if (!trimmedValue || value.includes(trimmedValue)) {
            setCustomValue("");
            return;
        }

        onChange([...value, trimmedValue]);
        setCustomValue("");
    };

    const handleCustomKeyDown = (event) => {
        if (event.key === "Enter") {
            event.preventDefault();
            addCustomOption();
        }
    };

    return (
        <div className="multi-select-field">
            <div className="multi-select">
                {options.map((option) => (
                    <button
                        key={option}
                        type="button"
                        className={
                            value.includes(option)
                                ? "multi-select-option selected"
                                : "multi-select-option"
                        }
                        onClick={() => toggleOption(option)}
                    >
                        {option}
                    </button>
                ))}

                {customOptions.map((option) => (
                    <button
                        key={option}
                        type="button"
                        className="multi-select-option selected"
                        onClick={() => toggleOption(option)}
                    >
                        {option}
                    </button>
                ))}

                {allowCustom && (
                    <div className="multi-select-option multi-select-option-input">
                        <span className="multi-select-option-plus">
                            +
                        </span>

                        <input
                            type="text"
                            value={customValue}
                            placeholder={customPlaceholder}
                            onChange={(event) =>
                                setCustomValue(event.target.value)
                            }
                            onKeyDown={handleCustomKeyDown}
                            onBlur={addCustomOption}
                            aria-label={customPlaceholder}
                        />
                    </div>
                )}
            </div>
        </div>
    );
}

export default MultiSelectInput;
