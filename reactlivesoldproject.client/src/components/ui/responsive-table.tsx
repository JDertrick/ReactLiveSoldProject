import * as React from "react";
import { cn } from "@/lib/utils";

interface ResponsiveTableProps extends React.HTMLAttributes<HTMLDivElement> {
  children: React.ReactNode;
}

/**
 * ResponsiveTable - A table wrapper that becomes card-based on mobile
 * On desktop: displays as normal table
 * On mobile: displays as stacked cards
 */
const ResponsiveTable = React.forwardRef<HTMLDivElement, ResponsiveTableProps>(
  ({ className, children, ...props }, ref) => (
    <div ref={ref} className={cn("w-full", className)} {...props}>
      {/* Desktop table - hidden on mobile */}
      <div className="hidden md:block overflow-auto rounded-md border">
        <table className="w-full caption-bottom text-sm">
          {children}
        </table>
      </div>

      {/* Mobile cards - hidden on desktop */}
      <div className="md:hidden space-y-4">
        {children}
      </div>
    </div>
  )
);
ResponsiveTable.displayName = "ResponsiveTable";

const ResponsiveTableHeader = React.forwardRef<
  HTMLTableSectionElement,
  React.HTMLAttributes<HTMLTableSectionElement>
>(({ className, ...props }, ref) => (
  <>
    {/* Desktop header */}
    <thead ref={ref} className={cn("hidden md:table-header-group [&_tr]:border-b", className)} {...props} />
    {/* No header needed for mobile cards */}
  </>
));
ResponsiveTableHeader.displayName = "ResponsiveTableHeader";

const ResponsiveTableBody = React.forwardRef<
  HTMLTableSectionElement,
  React.HTMLAttributes<HTMLTableSectionElement>
>(({ className, ...props }, ref) => (
  <tbody
    ref={ref}
    className={cn("md:[&_tr:last-child]:border-0", className)}
    {...props}
  />
));
ResponsiveTableBody.displayName = "ResponsiveTableBody";

interface ResponsiveTableRowProps extends React.HTMLAttributes<HTMLTableRowElement> {
  mobileCard?: React.ReactNode; // Custom mobile card layout
}

const ResponsiveTableRow = React.forwardRef<
  HTMLTableRowElement,
  ResponsiveTableRowProps
>(({ className, mobileCard, children, ...props }, ref) => (
  <>
    {/* Desktop row */}
    <tr
      ref={ref}
      className={cn(
        "hidden md:table-row border-b transition-colors hover:bg-muted/50 data-[state=selected]:bg-muted",
        className
      )}
      {...props}
    >
      {children}
    </tr>

    {/* Mobile card */}
    {mobileCard ? (
      <div className="md:hidden bg-white border rounded-lg p-4 shadow-sm">
        {mobileCard}
      </div>
    ) : (
      <div className="md:hidden bg-white border rounded-lg p-4 shadow-sm">
        {children}
      </div>
    )}
  </>
));
ResponsiveTableRow.displayName = "ResponsiveTableRow";

const ResponsiveTableHead = React.forwardRef<
  HTMLTableCellElement,
  React.ThHTMLAttributes<HTMLTableCellElement>
>(({ className, ...props }, ref) => (
  <th
    ref={ref}
    className={cn(
      "h-12 px-4 text-left align-middle font-medium text-muted-foreground [&:has([role=checkbox])]:pr-0",
      className
    )}
    {...props}
  />
));
ResponsiveTableHead.displayName = "ResponsiveTableHead";

interface ResponsiveTableCellProps extends React.TdHTMLAttributes<HTMLTableCellElement> {
  label?: string; // Label for mobile view
}

const ResponsiveTableCell = React.forwardRef<
  HTMLTableCellElement,
  ResponsiveTableCellProps
>(({ className, label, children, ...props }, ref) => (
  <>
    {/* Desktop cell */}
    <td
      ref={ref}
      className={cn("hidden md:table-cell p-4 align-middle [&:has([role=checkbox])]:pr-0", className)}
      {...props}
    >
      {children}
    </td>

    {/* Mobile cell - shown as labeled field */}
    {label && (
      <div className="md:hidden flex justify-between items-center py-2 border-b last:border-b-0">
        <span className="text-sm font-medium text-gray-600">{label}</span>
        <div className="text-sm">{children}</div>
      </div>
    )}

    {/* Mobile cell without label - shown as full width */}
    {!label && (
      <div className="md:hidden py-2">
        {children}
      </div>
    )}
  </>
));
ResponsiveTableCell.displayName = "ResponsiveTableCell";

const ResponsiveTableCaption = React.forwardRef<
  HTMLTableCaptionElement,
  React.HTMLAttributes<HTMLTableCaptionElement>
>(({ className, ...props }, ref) => (
  <caption
    ref={ref}
    className={cn("mt-4 text-sm text-muted-foreground", className)}
    {...props}
  />
));
ResponsiveTableCaption.displayName = "ResponsiveTableCaption";

export {
  ResponsiveTable,
  ResponsiveTableHeader,
  ResponsiveTableBody,
  ResponsiveTableRow,
  ResponsiveTableHead,
  ResponsiveTableCell,
  ResponsiveTableCaption,
};
