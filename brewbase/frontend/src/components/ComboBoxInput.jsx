import { useEffect, useMemo, useRef, useState } from "react";

function ComboBoxInput({
                           value,
                           onChange,
                           options,
                           placeholder,
                           id,
                           disabled = false,
                           allowCustom = true,
                       }) {
    const [isOpen, setIsOpen] = useState(false);
    const wrapperRef = useRef(null);

    const filteredOptions = useMemo(() => {
        if (!allowCustom) {
            return options;
        }

        const query = value.trim().toLowerCase();

        if (!query) {
            return options;
        }

        return options.filter((option) =>
            option.toLowerCase().includes(query)
        );
    }, [value, options, allowCustom]);

    useEffect(() => {
        const handleClickOutside = (event) => {
            if (
                wrapperRef.current &&
                !wrapperRef.current.contains(event.target)
            ) {
                setIsOpen(false);
            }
        };

        document.addEventListener("mousedown", handleClickOutside);

        return () => {
            document.removeEventListener("mousedown", handleClickOutside);
        };
    }, []);

    const selectOption = (option) => {
        onChange(option);
        setIsOpen(false);
    };

    return (
        <div className="combobox" ref={wrapperRef}>
            <input
                id={id}
                type="text"
                value={value}
                placeholder={placeholder}
                autoComplete="off"
                disabled={disabled}
                readOnly={!allowCustom}
                onChange={(event) => {
                    if (!allowCustom) {
                        return;
                    }

                    onChange(event.target.value);
                    setIsOpen(true);
                }}
                onFocus={() => {
                    if (!disabled) {
                        setIsOpen(true);
                    }
                }}
                onClick={() => {
                    if (!disabled) {
                        setIsOpen(true);
                    }
                }}
            />

            {isOpen && filteredOptions.length > 0 && (
                <ul className="combobox-dropdown" role="listbox">
                    {filteredOptions.map((option) => (
                        <li
                            key={option}
                            role="option"
                            className={
                                value === option
                                    ? "combobox-option selected"
                                    : "combobox-option"
                            }
                            onMouseDown={(event) => {
                                event.preventDefault();
                                selectOption(option);
                            }}
                        >
                            {option}
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}

export default ComboBoxInput;
