"""Unified mobile wallet payment flow for bKash, Rocket, and Nagad.

This module keeps the logic self-contained so it can be imported by any
Flask/FastAPI/Django endpoint or executed directly for manual testing.
"""

from __future__ import annotations

from dataclasses import dataclass, field, asdict
from datetime import datetime, timezone
import re
import uuid
from typing import Dict, Optional


MOBILE_NUMBER_PATTERN = re.compile(r"^01[3-9]\d{8}$")


class PaymentError(Exception):
	"""Base exception for payment related failures."""


class ValidationError(PaymentError):
	"""Raised when the incoming payload is not valid."""


class TransactionNotFoundError(PaymentError):
	"""Raised whenever someone queries an unknown transaction id."""


@dataclass(slots=True)
class PaymentRequest:
	"""Represents a generic send-money request."""

	provider: str
	sender_number: str
	receiver_number: str
	amount: float
	reference: str = ""
	sender_last_four: Optional[str] = None
	user_transaction_id: Optional[str] = None
	metadata: Dict[str, str] = field(default_factory=dict)

	def normalised_provider(self) -> str:
		return self.provider.strip().lower()


@dataclass(slots=True)
class PaymentReceipt:
	"""Successful payment response."""

	transaction_id: str
	provider: str
	sender_number: str
	receiver_number: str
	amount: float
	fee: float
	total_debited: float
	reference: str
	sender_last_four: Optional[str] = None
	user_transaction_id: Optional[str] = None
	metadata: Dict[str, str] = field(default_factory=dict)
	created_at: datetime = field(default_factory=lambda: datetime.now(timezone.utc))

	def masked_sender(self) -> str:
		return mask_number(self.sender_number)

	def masked_receiver(self) -> str:
		return mask_number(self.receiver_number)

	def as_dict(self) -> Dict[str, str | float]:
		payload = asdict(self)
		payload["created_at"] = self.created_at.isoformat() + "Z"
		payload["sender_number"] = self.masked_sender()
		payload["receiver_number"] = self.masked_receiver()
		return payload
		return payload


def mask_number(number: str) -> str:
	"""Mask all but the last four digits of a mobile wallet number."""

	number = number.strip()
	if len(number) < 4:
		return "*" * len(number)
	return f"{'*' * (len(number) - 4)}{number[-4:]}"


class PaymentProvider:
	"""Abstract provider with common validation logic."""

	code: str = ""
	display_name: str = ""
	service_charge_rate: float = 0.0
	min_amount: float = 10
	max_amount: float = 25000

	def __init__(self) -> None:
		self._ledger: Dict[str, PaymentReceipt] = {}

	# Public API ---------------------------------------------------------
	def send_money(self, request: PaymentRequest) -> PaymentReceipt:
		self._validate_request(request)
		fee = round(request.amount * self.service_charge_rate, 2)
		total = round(request.amount + fee, 2)
		transaction_id = self._generate_transaction_id()
		receipt = PaymentReceipt(
			transaction_id=transaction_id,
			provider=self.display_name,
			sender_number=request.sender_number,
			receiver_number=request.receiver_number,
			amount=request.amount,
			fee=fee,
			total_debited=total,
			reference=request.reference,
			sender_last_four=request.sender_last_four
			or request.sender_number[-4:],
			user_transaction_id=request.user_transaction_id,
			metadata=request.metadata.copy(),
		)
		self._ledger[transaction_id] = receipt
		return receipt

	def get_transaction(self, transaction_id: str) -> PaymentReceipt:
		try:
			return self._ledger[transaction_id]
		except KeyError as exc:
			raise TransactionNotFoundError(
				f"Transaction '{transaction_id}' not found for {self.display_name}"
			) from exc

	# Internal helpers ---------------------------------------------------
	def _validate_request(self, request: PaymentRequest) -> None:
		if request.amount < self.min_amount or request.amount > self.max_amount:
			raise ValidationError(
				f"Amount must be between {self.min_amount} and {self.max_amount} BDT"
			)

		for label, number in {
			"sender_number": request.sender_number,
			"receiver_number": request.receiver_number,
		}.items():
			if not MOBILE_NUMBER_PATTERN.match(number):
				raise ValidationError(
					f"{self.display_name}: {label} '{number}' is not a valid Bangladeshi MSISDN"
				)

		if request.sender_last_four and not request.sender_number.endswith(
			request.sender_last_four
		):
			raise ValidationError("Last four digits do not match the sender number")

	def _generate_transaction_id(self) -> str:
		identifier = uuid.uuid4().hex[:10].upper()
		return f"{self.code.upper()}-{identifier}"


class BkashProvider(PaymentProvider):
	code = "bkash"
	display_name = "bKash"
	service_charge_rate = 0.018
	min_amount = 20
	max_amount = 25000


class RocketProvider(PaymentProvider):
	code = "rocket"
	display_name = "Rocket"
	service_charge_rate = 0.015
	min_amount = 50
	max_amount = 50000


class NagadProvider(PaymentProvider):
	code = "nagad"
	display_name = "Nagad"
	service_charge_rate = 0.01
	min_amount = 20
	max_amount = 30000


class PaymentProcessor:
	"""Coordinator that dispatches to registered providers."""

	def __init__(self) -> None:
		self.providers: Dict[str, PaymentProvider] = {}

	def register_provider(self, provider: PaymentProvider) -> None:
		self.providers[provider.code] = provider

	def send(self, request: PaymentRequest) -> PaymentReceipt:
		provider = self._get_provider(request.normalised_provider())
		return provider.send_money(request)

	def lookup(self, provider_code: str, transaction_id: str) -> PaymentReceipt:
		provider = self._get_provider(provider_code.strip().lower())
		return provider.get_transaction(transaction_id)

	def _get_provider(self, provider_code: str) -> PaymentProvider:
		try:
			return self.providers[provider_code]
		except KeyError as exc:
			raise ValidationError(f"Unsupported provider '{provider_code}'") from exc


def bootstrap_processor() -> PaymentProcessor:
	"""Helper used by the admin/chat apps to reuse a single processor."""

	processor = PaymentProcessor()
	for provider in (BkashProvider(), RocketProvider(), NagadProvider()):
		processor.register_provider(provider)
	return processor


def _demo() -> None:
	processor = bootstrap_processor()
	demo_requests = [
		PaymentRequest(
			provider="bkash",
			sender_number="01712345678",
			receiver_number="01987654321",
			amount=1500,
			reference="Order #A102",
			sender_last_four="5678",
		),
		PaymentRequest(
			provider="rocket",
			sender_number="01899998888",
			receiver_number="01612344321",
			amount=3200,
			reference="Invoice 8842",
			sender_last_four="8888",
		),
		PaymentRequest(
			provider="nagad",
			sender_number="01577776666",
			receiver_number="01712000000",
			amount=550,
			reference="Top-up",
			sender_last_four="6666",
		),
	]

	for req in demo_requests:
		receipt = processor.send(req)
		print(f"\n✅ Sent via {receipt.provider}")
		for key, value in receipt.as_dict().items():
			print(f"  {key}: {value}")

	# lookup example using the last receipt
	lookup_provider = demo_requests[-1].provider
	lookup_id = receipt.transaction_id
	found = processor.lookup(lookup_provider, lookup_id)
	print("\n🔎 Transaction lookup result:")
	for key, value in found.as_dict().items():
		print(f"  {key}: {value}")


if __name__ == "__main__":
	_demo()
