import type { ReactNode, SVGProps } from "react";

type IconProps = SVGProps<SVGSVGElement>;

function base(children: ReactNode, props: IconProps) {
  return (
    <svg
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth={2}
      strokeLinecap="round"
      strokeLinejoin="round"
      width={18}
      height={18}
      {...props}
    >
      {children}
    </svg>
  );
}

export const HomeIcon = (props: IconProps) =>
  base(
    <>
      <path d="M3 11.5 12 4l9 7.5" />
      <path d="M5 10v9a1 1 0 0 0 1 1h4v-6h4v6h4a1 1 0 0 0 1-1v-9" />
    </>,
    props
  );

export const HistoryIcon = (props: IconProps) =>
  base(
    <>
      <path d="M3 12a9 9 0 1 0 3-6.7" />
      <path d="M3 4v5h5" />
      <path d="M12 7v5l3 3" />
    </>,
    props
  );

export const CheckSquareIcon = (props: IconProps) =>
  base(
    <>
      <rect x="3" y="3" width="18" height="18" rx="2" />
      <path d="m8 12 3 3 5-6" />
    </>,
    props
  );

export const ChartIcon = (props: IconProps) =>
  base(
    <>
      <path d="M4 20V10" />
      <path d="M12 20V4" />
      <path d="M20 20v-7" />
      <path d="M3 20h18" />
    </>,
    props
  );

export const GearIcon = (props: IconProps) =>
  base(
    <>
      <circle cx="12" cy="12" r="3" />
      <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06A1.65 1.65 0 0 0 4.68 15a1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06A1.65 1.65 0 0 0 9 4.6a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06A1.65 1.65 0 0 0 19.4 9c.14.36.44.63.8.72.36.1.7 0 .96-.24" />
    </>,
    props
  );

export const BellIcon = (props: IconProps) =>
  base(
    <>
      <path d="M6 8a6 6 0 0 1 12 0c0 5 2 6 2 6H4s2-1 2-6" />
      <path d="M10 21a2 2 0 0 0 4 0" />
    </>,
    props
  );

export const PlusIcon = (props: IconProps) =>
  base(
    <>
      <path d="M12 5v14" />
      <path d="M5 12h14" />
    </>,
    props
  );

export const ArrowRightIcon = (props: IconProps) =>
  base(
    <>
      <path d="M5 12h14" />
      <path d="m13 5 7 7-7 7" />
    </>,
    props
  );

export const ChevronRightIcon = (props: IconProps) =>
  base(<path d="m9 18 6-6-6-6" />, props);

export const SearchIcon = (props: IconProps) =>
  base(
    <>
      <circle cx="11" cy="11" r="7" />
      <path d="m21 21-4.3-4.3" />
    </>,
    props
  );

export const XIcon = (props: IconProps) => base(<path d="M18 6 6 18M6 6l12 12" />, props);

export const ThumbsUpIcon = (props: IconProps) =>
  base(
    <>
      <path d="M7 22V11" />
      <path d="M3 11h4v11H3z" />
      <path d="M7 11l3.6-7.5A1.6 1.6 0 0 1 12 3a2 2 0 0 1 2 2.2L13.2 11H19a2 2 0 0 1 2 2.4l-1.6 7A2 2 0 0 1 17.4 22H7" />
    </>,
    props
  );

export const MessageIcon = (props: IconProps) =>
  base(<path d="M21 11.5a8.38 8.38 0 0 1-8.5 8.5A8.5 8.5 0 1 1 21 11.5Z" />, props);

export const CalendarIcon = (props: IconProps) =>
  base(
    <>
      <rect x="3" y="4" width="18" height="18" rx="2" />
      <path d="M16 2v4M8 2v4M3 10h18" />
    </>,
    props
  );

export const UsersIcon = (props: IconProps) =>
  base(
    <>
      <circle cx="9" cy="8" r="3.5" />
      <path d="M2.5 20a6.5 6.5 0 0 1 13 0" />
      <path d="M16 4.5c1.66 0 3 1.34 3 3s-1.34 3-3 3" />
      <path d="M18 13.5a5 5 0 0 1 3.5 4.8" />
    </>,
    props
  );

export const ClockIcon = (props: IconProps) =>
  base(
    <>
      <circle cx="12" cy="12" r="9" />
      <path d="M12 7v5l3 3" />
    </>,
    props
  );

export const StarIcon = (props: IconProps) =>
  base(
    <path d="m12 3 2.6 5.9 6.4.6-4.8 4.3 1.4 6.3L12 17l-5.6 3.1 1.4-6.3-4.8-4.3 6.4-.6Z" />,
    props
  );

export const RefreshIcon = (props: IconProps) =>
  base(
    <>
      <path d="M3 12a9 9 0 0 1 15.3-6.4L21 8" />
      <path d="M21 3v5h-5" />
      <path d="M21 12a9 9 0 0 1-15.3 6.4L3 16" />
      <path d="M3 21v-5h5" />
    </>,
    props
  );

export const FilterIcon = (props: IconProps) =>
  base(<path d="M4 5h16l-6 8v6l-4 2v-8L4 5Z" />, props);

export const CheckIcon = (props: IconProps) => base(<path d="M20 6 9 17l-5-5" />, props);

export const LayersIcon = (props: IconProps) =>
  base(
    <>
      <path d="m12 2 9 5-9 5-9-5 9-5Z" />
      <path d="m3 12 9 5 9-5" />
      <path d="m3 17 9 5 9-5" />
    </>,
    props
  );

export const PencilIcon = (props: IconProps) =>
  base(
    <>
      <path d="M12 20h9" />
      <path d="M16.5 3.5a2.12 2.12 0 0 1 3 3L7 19l-4 1 1-4Z" />
    </>,
    props
  );

export const LogOutIcon = (props: IconProps) =>
  base(
    <>
      <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
      <path d="M16 17l5-5-5-5" />
      <path d="M21 12H9" />
    </>,
    props
  );
