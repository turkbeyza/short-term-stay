"use client";

import { useState, useRef, useEffect } from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import {
  Send, MapPin, Calendar, Star, Hotel, Bot, User,
  DollarSign, ArrowRight, Users,
} from "lucide-react";
import { motion, AnimatePresence } from "framer-motion";

type Listing = {
  id: number;
  city: string;
  country: string;
  price: number;
  noOfPeople: number;
  rating: number;
};

type Message = {
  id: string;
  role: "user" | "assistant";
  content: string;
};

export default function ChatPage() {
  const [messages, setMessages] = useState<Message[]>([
    {
      id: "1",
      role: "assistant",
      content:
        "Hello! I'm your **Stay AI Agent**. I can help you find listings, book your stay, or leave a review.\n\nWhat would you like to do today?",
    },
  ]);
  const [input, setInput] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [sessionId, setSessionId] = useState("");
  const messagesEndRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, isLoading]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!input.trim() || isLoading) return;

    const userMsg: Message = {
      id: Date.now().toString(),
      role: "user",
      content: input.trim(),
    };
    setMessages((prev) => [...prev, userMsg]);
    setInput("");
    setIsLoading(true);

    try {
      const res = await fetch("http://localhost:5006/api/v1/chat", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ message: userMsg.content, sessionId }),
      });

      if (!res.ok) throw new Error("Request failed");

      const data = await res.json();
      if (data.sessionId && !sessionId) setSessionId(data.sessionId);

      setMessages((prev) => [
        ...prev,
        {
          id: (Date.now() + 1).toString(),
          role: "assistant",
          content: data.reply,
        },
      ]);
    } catch (err: any) {
      console.error("Chat error:", err);
      setMessages((prev) => [
        ...prev,
        {
          id: (Date.now() + 1).toString(),
          role: "assistant",
          content: `Connection Error: ${err.message || "Unknown error"}. Check console for details.`,
        },
      ]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSubmit(e as unknown as React.FormEvent);
    }
  };

  return (
    <div className="flex h-dvh bg-neutral-950 text-neutral-100 overflow-hidden" style={{ fontFamily: "'Inter', sans-serif" }}>

      {/* ── Sidebar ── */}
      <aside className="hidden lg:flex flex-col w-72 shrink-0 bg-neutral-900 border-r border-neutral-800/70">
        {/* Logo */}
        <div className="px-6 py-6 border-b border-neutral-800/70">
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-xl bg-indigo-500/15 ring-1 ring-indigo-500/30">
              <Hotel className="w-5 h-5 text-indigo-400" />
            </div>
            <span className="text-base font-semibold bg-gradient-to-r from-indigo-400 via-blue-400 to-white bg-clip-text text-transparent">
              Stay AI Agent
            </span>
          </div>
        </div>

        {/* Capabilities */}
        <div className="flex-1 px-4 py-5 overflow-y-auto">
          <p className="px-2 mb-3 text-[10px] uppercase tracking-widest font-semibold text-neutral-500">
            Capabilities
          </p>
          <div className="space-y-1">
            <SidebarItem icon={<MapPin size={15} />} label="Find Stays" sub="Search across cities & countries" />
            <SidebarItem icon={<Calendar size={15} />} label="Book Listings" sub="Instantly reserve your dates" />
            <SidebarItem icon={<Star size={15} />} label="Leave Reviews" sub="Rate your past experiences" />
          </div>
        </div>

        {/* Session debug */}
        <div className="px-6 py-4 border-t border-neutral-800/70">
          <p className="text-[10px] text-neutral-600 font-mono break-all">
            Session: {sessionId || "—"}
          </p>
        </div>
      </aside>

      {/* ── Main ── */}
      <div className="flex-1 flex flex-col min-w-0 relative">

        {/* Ambient background */}
        <div className="pointer-events-none absolute inset-0 overflow-hidden">
          <div className="absolute -top-32 -left-32 w-96 h-96 bg-indigo-600/8 rounded-full blur-3xl" />
          <div className="absolute -bottom-32 -right-32 w-96 h-96 bg-purple-600/8 rounded-full blur-3xl" />
        </div>

        {/* Messages scroll area */}
        <div className="flex-1 overflow-y-auto relative z-10 custom-scrollbar">
          <div className="max-w-3xl mx-auto px-4 py-6 space-y-6">
            <AnimatePresence initial={false}>
              {messages.map((m) => (
                <MessageBubble key={m.id} message={m} setInput={setInput} />
              ))}

              {isLoading && (
                <motion.div
                  key="loading"
                  initial={{ opacity: 0, y: 8 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0 }}
                  className="flex gap-3"
                >
                  <Avatar assistant />
                  <div className="bg-neutral-900 border border-neutral-800 rounded-2xl rounded-tl-sm px-4 py-3 flex items-center gap-2.5">
                    <span className="flex gap-1">
                      {[0, 0.18, 0.36].map((d, i) => (
                        <span
                          key={i}
                          className="block w-1.5 h-1.5 rounded-full bg-indigo-400 animate-bounce"
                          style={{ animationDelay: `${d}s` }}
                        />
                      ))}
                    </span>
                    <span className="text-sm text-neutral-500">Thinking…</span>
                  </div>
                </motion.div>
              )}
            </AnimatePresence>
            <div ref={messagesEndRef} />
          </div>
        </div>

        {/* Input bar — pinned to bottom, no extra gap */}
        <div className="relative z-10 border-t border-neutral-800/60 bg-neutral-950/90 backdrop-blur-xl px-4 py-3">
          <div className="max-w-3xl mx-auto">
            <form
              onSubmit={handleSubmit}
              className="flex items-end gap-3 bg-neutral-900 border border-neutral-800 rounded-2xl px-4 py-3 focus-within:border-indigo-500/50 transition-colors"
            >
              <textarea
                rows={1}
                value={input}
                onChange={(e) => {
                  setInput(e.target.value);
                  e.target.style.height = "auto";
                  e.target.style.height = Math.min(e.target.scrollHeight, 120) + "px";
                }}
                onKeyDown={handleKeyDown}
                placeholder="Ask me to find a stay, book a listing, or leave a review…"
                disabled={isLoading}
                className="flex-1 resize-none bg-transparent outline-none text-sm text-neutral-100 placeholder-neutral-600 leading-relaxed max-h-32 overflow-y-auto custom-scrollbar"
                style={{ height: "24px" }}
              />
              <button
                type="submit"
                disabled={!input.trim() || isLoading}
                className="shrink-0 bg-indigo-500 hover:bg-indigo-400 disabled:bg-neutral-800 disabled:text-neutral-600 text-white p-2 rounded-xl transition-all active:scale-95"
              >
                <Send size={16} />
              </button>
            </form>
            <p className="text-center text-[10px] text-neutral-700 mt-2">
              Press Enter to send · Shift+Enter for new line
            </p>
          </div>
        </div>
      </div>

      <style>{`
        @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap');
        .custom-scrollbar::-webkit-scrollbar { width: 4px; }
        .custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
        .custom-scrollbar::-webkit-scrollbar-thumb { background: #2a2a2a; border-radius: 99px; }
        .custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #3f3f3f; }

        .prose-chat { font-size: 14.5px; line-height: 1.7; color: #d4d4d4; }
        .prose-chat p { margin: 0 0 0.5em; }
        .prose-chat p:last-child { margin-bottom: 0; }
        .prose-chat strong { color: #a5b4fc; font-weight: 600; }
        .prose-chat em { color: #c4b5fd; }
        .prose-chat code { background: #1e1e2e; color: #a5b4fc; padding: 1px 6px; border-radius: 4px; font-size: 13px; }
        .prose-chat pre { background: #1e1e2e; border: 1px solid #2d2d3a; border-radius: 10px; padding: 12px 16px; overflow-x: auto; margin: 8px 0; }
        .prose-chat pre code { background: none; padding: 0; }
        .prose-chat ul, .prose-chat ol { padding-left: 1.4em; margin: 4px 0 8px; }
        .prose-chat li { margin: 2px 0; }
        .prose-chat h1, .prose-chat h2, .prose-chat h3 { color: #e5e5e5; font-weight: 600; margin: 12px 0 4px; }
        .prose-chat h1 { font-size: 1.1em; }
        .prose-chat h2 { font-size: 1em; }
        .prose-chat h3 { font-size: 0.95em; }
        .prose-chat blockquote { border-left: 3px solid #4f46e5; padding-left: 12px; color: #a3a3a3; margin: 8px 0; }
        .prose-chat a { color: #818cf8; text-decoration: underline; }
        .prose-chat hr { border-color: #2d2d3a; margin: 12px 0; }
        .prose-chat table { width: 100%; border-collapse: collapse; font-size: 13px; }
        .prose-chat th, .prose-chat td { border: 1px solid #2d2d3a; padding: 6px 10px; text-align: left; }
        .prose-chat th { background: #1e1e2e; color: #a5b4fc; }
      `}</style>
    </div>
  );
}

// ── Avatar ──────────────────────────────────────────────
function Avatar({ assistant }: { assistant?: boolean }) {
  return (
    <div
      className={`w-8 h-8 rounded-full shrink-0 flex items-center justify-center shadow-lg ${assistant
        ? "bg-gradient-to-br from-blue-400 to-white-400"
        : "bg-neutral-800 border border-neutral-700"
        }`}
    >
      {assistant ? (
        <Bot size={15} className="text-white" />
      ) : (
        <User size={15} className="text-neutral-400" />
      )}
    </div>
  );
}

// ── MessageBubble ────────────────────────────────────────
function MessageBubble({
  message,
  setInput,
}: {
  message: Message;
  setInput: (s: string) => void;
}) {
  const isUser = message.role === "user";

  // Strip and parse listing JSON block
  let cleanContent = message.content;
  let listings: Listing[] = [];

  const match = message.content.match(/\[LISTINGS_JSON\]([\s\S]*?)\[\/LISTINGS_JSON\]/);
  if (match) {
    try {
      listings = JSON.parse(match[1]);
    } catch {
      /* ignore parse errors */
    }
    cleanContent = message.content
      .replace(/\[LISTINGS_JSON\][\s\S]*?\[\/LISTINGS_JSON\]/, "")
      .trim();
  }

  return (
    <motion.div
      initial={{ opacity: 0, y: 10 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.2 }}
      className={`flex gap-3 ${isUser ? "flex-row-reverse" : ""}`}
    >
      <Avatar assistant={!isUser} />

      <div className={`flex flex-col gap-3 ${isUser ? "items-end max-w-[80%]" : "flex-1 min-w-0"}`}>
        {/* Bubble */}
        <div
          className={`rounded-2xl px-4 py-3 text-sm leading-relaxed ${isUser
            ? "bg-indigo-600/20 border border-indigo-500/30 text-neutral-100 rounded-tr-sm"
            : "bg-neutral-900 border border-neutral-800 rounded-tl-sm"
            }`}
        >
          {isUser ? (
            <p className="text-sm leading-relaxed whitespace-pre-wrap">{cleanContent}</p>
          ) : (
            <div className="prose-chat">
              <ReactMarkdown remarkPlugins={[remarkGfm]}>{cleanContent}</ReactMarkdown>
            </div>
          )}
        </div>

        {/* Listing cards */}
        {listings.length > 0 && (
          <div className="w-full grid grid-cols-1 sm:grid-cols-2 gap-3">
            {listings.map((listing) => (
              <ListingCard
                key={listing.id}
                listing={listing}
                onBook={() =>
                  setInput(`I want to book listing ${listing.id} in ${listing.city}. Please help me complete the booking.`)
                }
              />
            ))}
          </div>
        )}
      </div>
    </motion.div>
  );
}

// ── ListingCard ──────────────────────────────────────────
function ListingCard({
  listing,
  onBook,
}: {
  listing: Listing;
  onBook: () => void;
}) {
  return (
    <motion.div
      whileHover={{ y: -3 }}
      transition={{ duration: 0.15 }}
      className="group bg-neutral-900 border border-neutral-800 rounded-xl overflow-hidden hover:border-indigo-500/40 transition-colors"
    >
      <div className="p-4">
        {/* Header row */}
        <div className="flex items-start justify-between gap-2 mb-3">
          <div>
            <h4 className="font-semibold text-neutral-100 text-sm group-hover:text-indigo-400 transition-colors">
              {listing.city}
            </h4>
            <p className="text-xs text-neutral-500 mt-0.5">{listing.country}</p>
          </div>
          <div className="flex items-center gap-1 bg-yellow-500/10 text-yellow-400 px-2 py-1 rounded-lg text-xs font-semibold shrink-0">
            <Star size={10} fill="currentColor" />
            {listing.rating > 0 ? listing.rating.toFixed(1) : "New"}
          </div>
        </div>

        {/* Meta pills */}
        <div className="flex gap-2 flex-wrap mb-4">
          <span className="inline-flex items-center gap-1 text-xs text-neutral-400 bg-neutral-800 px-2.5 py-1 rounded-lg">
            <Users size={11} />
            Up to {listing.noOfPeople}
          </span>
          <span className="inline-flex items-center gap-1 text-xs font-semibold text-indigo-400 bg-indigo-500/10 px-2.5 py-1 rounded-lg">
            <DollarSign size={11} />
            {listing.price} / night
          </span>
        </div>

        {/* CTA */}
        <button
          onClick={onBook}
          className="w-full py-2 rounded-lg bg-neutral-800 hover:bg-indigo-600 text-neutral-300 hover:text-white text-xs font-semibold transition-all flex items-center justify-center gap-1.5 group/btn"
        >
          Book this stay
          <ArrowRight size={12} className="group-hover/btn:translate-x-0.5 transition-transform" />
        </button>
      </div>
    </motion.div>
  );
}

// ── SidebarItem ──────────────────────────────────────────
function SidebarItem({
  icon,
  label,
  sub,
}: {
  icon: React.ReactNode;
  label: string;
  sub: string;
}) {
  return (
    <div className="flex items-center gap-3 px-2 py-2.5 rounded-xl hover:bg-neutral-800/60 transition-colors cursor-default group">
      <div className="p-1.5 rounded-lg bg-neutral-800 text-neutral-500 group-hover:bg-indigo-500/15 group-hover:text-indigo-400 transition-colors">
        {icon}
      </div>
      <div>
        <p className="text-xs font-medium text-neutral-300">{label}</p>
        <p className="text-[11px] text-neutral-600 mt-0.5">{sub}</p>
      </div>
    </div>
  );
}
