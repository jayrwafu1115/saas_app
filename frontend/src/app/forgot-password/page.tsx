"use client";

import Link from "next/link";
import { FormEvent, useState } from "react";
import { Loader2, MailQuestion } from "lucide-react";
import { Button } from "@/components/ui/button";
import { forgotPassword } from "@/lib/api";

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [resetToken, setResetToken] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);
    const result = await forgotPassword({ email });
    setResetToken(result.resetToken ?? "");
    setIsSubmitting(false);
  }

  return (
    <main className="flex min-h-screen items-center justify-center bg-background px-6 text-foreground">
      <form onSubmit={handleSubmit} className="w-full max-w-md rounded-md border border-border bg-surface p-6">
        <div className="flex items-center gap-3 border-b border-border pb-4">
          <MailQuestion className="h-5 w-5 text-accent" aria-hidden="true" />
          <h1 className="text-xl font-semibold">Forgot Password</h1>
        </div>
        <div className="mt-5 grid gap-4">
          <label className="grid gap-2 text-sm font-medium">
            Email
            <input className="h-10 rounded-md border border-border bg-background px-3" value={email} onChange={(event) => setEmail(event.target.value)} type="email" required />
          </label>
          {resetToken ? (
            <label className="grid gap-2 text-sm font-medium">
              Reset token
              <textarea className="min-h-24 rounded-md border border-border bg-background px-3 py-2 font-mono text-xs" value={resetToken} onChange={(event) => setResetToken(event.target.value)} />
            </label>
          ) : null}
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" /> : null}
            Send Reset
          </Button>
          <Button type="button" variant="outline" asChild>
            <Link href={`/reset-password?email=${encodeURIComponent(email)}&token=${encodeURIComponent(resetToken)}`}>Reset password</Link>
          </Button>
        </div>
      </form>
    </main>
  );
}
