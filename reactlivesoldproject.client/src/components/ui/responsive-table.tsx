import * as React from "react";
import { cn } from "@/lib/utils";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "./table";

interface ResponsiveTableProps extends React.HTMLAttributes<HTMLDivElement> {
  children: React.ReactNode;
}

/**
 * ResponsiveTable - A wrapper around shadcn Table that provides responsive behavior
 * On desktop: displays as normal table
 * On mobile: children should handle their own mobile layout
 */
const ResponsiveTable = React.forwardRef<HTMLDivElement, ResponsiveTableProps>(
  ({ className, children, ...props }, ref) => (
    <div ref={ref} className={cn("w-full", className)} {...props}>
      <div className="rounded-md border">
        <Table>
          {children}
        </Table>
      </div>
    </div>
  )
);
ResponsiveTable.displayName = "ResponsiveTable";

// Re-export shadcn table components for convenience
const ResponsiveTableHeader = TableHeader;
ResponsiveTableHeader.displayName = "ResponsiveTableHeader";

const ResponsiveTableBody = TableBody;
ResponsiveTableBody.displayName = "ResponsiveTableBody";

const ResponsiveTableRow = TableRow;
ResponsiveTableRow.displayName = "ResponsiveTableRow";

const ResponsiveTableHead = TableHead;
ResponsiveTableHead.displayName = "ResponsiveTableHead";

const ResponsiveTableCell = TableCell;
ResponsiveTableCell.displayName = "ResponsiveTableCell";

export {
  ResponsiveTable,
  ResponsiveTableHeader,
  ResponsiveTableBody,
  ResponsiveTableRow,
  ResponsiveTableHead,
  ResponsiveTableCell,
};
